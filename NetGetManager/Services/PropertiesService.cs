using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetGetShared;
using DataLibrary;

namespace NetGetManager
{
    public class PropertiesService
    {
        private readonly DataAccessMySQL DataAccessMySQL;
        private readonly DataAccessSQLite DataAccessSQLite;
        private readonly RefreshService RefreshService;
        private readonly CrudService CrudService;

        public UserModel ThisUser { get; set; }
        public List<UserModel> Users { get; set; }
        public UserModel SelectedUser { get; set; }
        public List<string> TemplateTables { get; set; }
        public List<string> ContentTables { get; set; }
        public List<GroupModel> Groups { get; set; }
        public List<TemplateModel> Templates { get; set; }
        public List<ContentModel> Downloads { get; set; }
        public List<ContentModel> Wingets { get; set; }
        public ContentModel SelectedAvailableContent { get; set; }
        public ContentModel SelectedContent { get; set; }
        public bool NavMenuToggle { get; set; }
        public RepoSettings RepoSettings { get; set; }
        public string RootName
        {
            get
            {
                return RepoSettings.RootName;
            }
        }

        public PropertiesService(DataAccessMySQL dataAccessMySQL, DataAccessSQLite dataAccessSQLite, RefreshService refreshService, CrudService crudService)
        {
            DataAccessSQLite = dataAccessSQLite;
            DataAccessSQLite.Cs = new ConnectionStringSQLite(Path.Combine(Path.GetTempPath(), @"NetGet\wingetRepo\Public\index.db"));
            DataAccessMySQL = dataAccessMySQL;
            RefreshService = refreshService;
            CrudService = crudService;
            Users = new();
            Groups = new();
            Templates = new();
            TemplateTables = new();
            ContentTables = new();
            Downloads = new();
            Wingets = new();
            RepoSettings = new();
            NavMenuToggle = false;
        }

        public async Task Initialize()
        {
            ThisUser = await CrudService.FindUser(DataAccessMySQL.Cs.GetProperties()[1]);
            await GetUserPrivileges(ThisUser);
            await RefreshRepoSettings();
            await CrudService.CreateTables(ThisUser, RootName);
            await RefreshUsers();
            await RefreshTables();
            await RefreshGroups();
            await RefreshTemplates();
            await RefreshDownloads();
            await RefreshWingets();
            NavMenuToggle = true;
            RefreshService.CallNavMenuRefreshRequested();
            RefreshService.CallViewRefreshRequested();
        }
        public void ClearSelection()
        {
            SelectedUser = null;
            SelectedAvailableContent = null;
            SelectedContent = null;
        }
        public async Task RefreshUsers()
        {
            List<UserModel> users = await CrudService.ReadUsers();
            users = users.OrderBy(x => x.Name).ToList();
            foreach (UserModel user in users)
            {
                await GetUserPrivileges(user);
            }
            users.RemoveAll(x => x.Type is null);
            Users = users;
            RefreshService.CallNavMenuRefreshRequested();
        }
        public async Task RefreshTables()
        {
            if (ThisUser.Type == "root" || ThisUser.Type == "admin")
            {
                TemplateTables = await CrudService.FindTables("%template");
                ContentTables = await CrudService.FindTables("%content");
            }
            else
            {
                TemplateTables = await CrudService.FindTables($"{ThisUser.Name}_template");
                TemplateTables.AddRange(await CrudService.FindTables($"{RootName}_template"));
                ContentTables = await CrudService.FindTables($"{ThisUser.Name}_content");
                ContentTables.AddRange(await CrudService.FindTables($"{RootName}_content"));
            }
        }
        public async Task RefreshGroups()
        {
            Groups = (await CrudService.ReadGroups()).OrderBy(x => x.Name).ToList();
            await CrudService.UpdateClientsPrivileges(Users.Where(x => x.Type == "client"), Groups);
        }
        public async Task RefreshTemplates()
        {
            List<TemplateModel> templates = new();
            templates = await CrudService.ReadTemplates(TemplateTables, RootName);
            templates = templates.OrderBy(x => x.Name).ToList();
            Templates = templates;

        }
        public async Task RefreshDownloads()
        {
            Downloads = await CrudService.ReadDownloads(ContentTables);
        }
        public async Task RefreshWingets()
        {
            Wingets = await CrudService.ReadWingets();
        }
        public async Task RefreshRepoSettings()
        {
            if (ThisUser.Type == "root")
                await CrudService.UpdateRootName(ThisUser.Name);
            RepoSettings = await CrudService.ReadRepoSettings();
        }
        private async Task GetUserPrivileges(UserModel user)
        {
            List<string> privileges = await CrudService.ReadPrivileges(user);

            List<string> rootComperator = new()
            {
                $"GRANT ALL PRIVILEGES ON *.* TO `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}` IDENTIFIED BY PASSWORD '{user.Password}' WITH GRANT OPTION",
                $"GRANT PROXY ON ''@'%' TO '{user.Name}'@'{DataAccessMySQL.Cs.GetProperties()[0]}' WITH GRANT OPTION"
            };

            List<string> adminComperator = new()
            {
                $"GRANT ALL PRIVILEGES ON `{DataAccessMySQL.Cs.GetProperties()[3]}`.* TO `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}` WITH GRANT OPTION",
                $"GRANT ALL PRIVILEGES ON `mysql`.* TO `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`"
            };

            if (rootComperator.All(x => privileges.Contains(x)))
            {
                user.Type = "root";
            }
            else if (adminComperator.All(x => privileges.Contains(x)))
            {
                user.Type = "admin";
            }
            else if (privileges.Where(x => x.StartsWith($"GRANT ALL PRIVILEGES ON `{DataAccessMySQL.Cs.GetProperties()[3]}`.")).Count() > 0)
            {
                user.Type = "curator";
            }
            else if (privileges.Where(x => x.StartsWith($"GRANT SELECT ON `{DataAccessMySQL.Cs.GetProperties()[3]}`.")).Count() > 0)
            {
                user.Type = "client";
            }
        }

    }
}
