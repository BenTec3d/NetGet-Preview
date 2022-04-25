using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;

namespace NetGetShared
{
    public class CrudService
    {
        private readonly DataAccessMySQL DataAccessMySQL;
        private readonly DataAccessSQLite DataAccessSQLite;

        public CrudService(DataAccessMySQL dataAccessMySQL, DataAccessSQLite dataAccessSQLite)
        {
            DataAccessMySQL = dataAccessMySQL;
            DataAccessSQLite = dataAccessSQLite;
        }

        /*CONTENT*/
        public async Task CreateContent(ContentModel content)
        {
            string sql = $"INSERT INTO {content.User}_content VALUES(null, @Type, @Name, @Publisher, @Version, @Command, @Url, @Instructions)";
            await DataAccessMySQL.SaveData(sql, new { Type = content.Type, Name = content.Name, Publisher = content.Publisher, Version = content.Version, Command = content.Command, Url = content.Url, Instructions = content.Instructions });
        }
        public async Task DeleteContent(ContentModel content)
        {
            string sql = $"DELETE FROM {content.User}_content WHERE Id = @Id";
            await DataAccessMySQL.SaveData(sql, new { Id = content.Id });
        }
        public async Task DeleteContent(List<ContentModel> contentList)
        {
            List<string> sql = new();
            foreach (ContentModel content in contentList)
            {
                sql.Add($"DELETE FROM {content.User}_content WHERE Id = {content.Id}");
            }
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task<ContentModel> FindContent(ContentModel content)
        {
            string sql = $"SELECT *, '{content.User}' AS User FROM {content.User}_content WHERE Id = {content.Id}";
            return (await DataAccessMySQL.LoadData<ContentModel>(sql)).ElementAt(0);
        }
        public async Task<ContentModel> FindContent(GroupModel group, ContentModel content)
        {
            string sql = $"SELECT * FROM {group.Name}_group WHERE Id = {content.Id}";
            return (await DataAccessMySQL.LoadData<ContentModel>(sql)).ElementAt(0);
        }
        public async Task<List<ContentModel>> ReadContent(TemplateModel template, string root)
        {
            string[] sql =
            {
                $"SELECT {template.User}_content.Id, Type, {template.User}_content.Name, '{template.User}' AS User, Publisher, Version, Command, Url, Instructions FROM {template.User}_content JOIN {template.User}_tempcont ON {template.User}_tempcont.UserContentId = {template.User}_content.Id WHERE TemplateId = {template.Id}",
                $"SELECT {root}_content.Id, Type, {root}_content.Name, '{root}' AS User, Publisher, Version, Command, Url, Instructions FROM {root}_content JOIN {template.User}_tempcont ON {template.User}_tempcont.RootContentId = {root}_content.Id WHERE TemplateId = {template.Id}"
            };
            return await DataAccessMySQL.LoadData<ContentModel>(sql);
        }
        public async Task<List<ContentModel>> ReadContent(GroupModel group)
        {
            string sql = $"SELECT * FROM {group.Name}_group";
            return await DataAccessMySQL.LoadData<ContentModel>(sql);
        }
        public async Task UpdateContent(ContentModel oldContent, ContentModel newContent)
        {
            string sql = $"UPDATE {oldContent.User}_content SET Type = @Type, Name = @Name, Publisher = @Publisher, Version = @Version, Command = @Command, Url = @Url, Instructions = @Instructions WHERE Id = @Id";
            await DataAccessMySQL.SaveData(sql, new { Type = newContent.Type, Name = newContent.Name, Publisher = newContent.Publisher, Version = newContent.Version, Command = newContent.Command, Url = newContent.Url, Instructions = newContent.Instructions, Id = oldContent.Id });
        }
        public async Task UpdateContent(GroupModel group, ContentModel oldContent, ContentModel newContent)
        {
            string sql = $"UPDATE {group.Name}_group SET Type = @Type, Name = @Name, Publisher = @Publisher, Version = @Version, Command = @Command, Url = @Url, User = @User, Instructions = @Instructions WHERE Id = @Id";
            await DataAccessMySQL.SaveData(sql, new { Type = newContent.Type, Name = newContent.Name, Publisher = newContent.Publisher, Version = newContent.Version, Command = newContent.Command, Url = newContent.Url, User = newContent.User, Instructions = newContent.Instructions, Id = oldContent.Id });
        }

        /*DOWNLOADS*/
        public async Task<List<ContentModel>> ReadDownloads(string table)
        {
            return await DataAccessMySQL.LoadData<ContentModel>($"SELECT Id, Type, Name, '{table.Remove(table.Length - 8)}' AS User, Url, Instructions FROM {table} WHERE Type = 'download'");
        }
        public async Task<List<ContentModel>> ReadDownloads(List<string> Tables)
        {
            List<ContentModel> content = new();
            foreach (string table in Tables)
            {
                content.AddRange(await ReadDownloads(table));
            }
            return content;
        }

        /*GROUPS*/
        public async Task AddContentToGroup(ContentModel content, GroupModel group)
        {
            string sql = $"INSERT INTO {group.Name}_group VALUES (null, @Type, @Name, @Publisher, @Version, @Command, @Url, @User, @Instructions)";
            await DataAccessMySQL.SaveData(sql, new { Type = content.Type, Name = content.Name, Publisher = content.Publisher, Version = content.Version, Command = content.Command, Url = content.Url, User = content.User, Instructions = content.Instructions });
        }
        public async Task AddContentToGroup(List<ContentModel> contentList, GroupModel group)
        {
            List<string> sql = new();
            foreach (ContentModel content in contentList)
            {
                string s = $"INSERT INTO {group.Name}_group VALUES (null, '{content.Type}', '{content.Name}', '{content.Publisher}', '{content.Version}', '{content.Command}', '{content.Url}', '{content.User}', '{content.Instructions}')";
                sql.Add(s.Replace("''", "null"));
            }
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task CreateGroup(string name)
        {
            string sql = $"CREATE TABLE IF NOT EXISTS `{name}_group`(`Id` int(11) NOT NULL AUTO_INCREMENT,`Type` VARCHAR(50) NOT NULL,`Name` VARCHAR(50) NOT NULL,`Publisher` TEXT,`Version` TEXT,`Command` TEXT,`Url` TEXT,`User` TEXT,`Instructions` TEXT, PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=latin1";
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task DeleteGroup(GroupModel group, IEnumerable<UserModel> clients)
        {
            List<string> sql = new();
            foreach (UserModel client in clients)
            {
                sql.Add($"REVOKE SELECT ON {group.Name}_group FROM `{client.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
            }
            sql.Add($"DROP TABLE {group.Name}_group");
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task<List<GroupModel>> ReadGroups()
        {
            List<string> groupNames = new();
            List<GroupModel> groups = new();
            groupNames = await FindTables("%_group");
            foreach (string groupName in groupNames)
            {
                groups.Add(new GroupModel(groupName.Replace("_group", "")));
            }
            foreach (GroupModel group in groups)
            {
                group.Content = await ReadContent(group);
            }
            return groups;
        }
        public async Task UpdateGroup(GroupModel group, string name)
        {
            string sql = $"ALTER TABLE {group.Name}_group RENAME {name + "_group"}";
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task RemoveContentFromGroup(GroupModel group, ContentModel content)
        {
            string sql = $"DELETE FROM {group.Name}_group WHERE Id = @Id";
            await DataAccessMySQL.SaveData(sql, new { Id = content.Id });
        }
        public async Task RemoveContentFromGroup(GroupModel group, List<ContentModel> contentList)
        {
            List<string> sql = new();
            foreach (ContentModel content in contentList)
            {
                sql.Add($"DELETE FROM {group.Name}_group WHERE Id = {content.Id}");
            }
            await DataAccessMySQL.SaveData(sql, new { });
        }

        /*PRIVILEGES*/
        public async Task CreatePrivileges(string user, string type, string root)
        {
            List<string> sql = new();
            if (type == "admin")
            {
                sql.Add($"GRANT ALL PRIVILEGES ON {DataAccessMySQL.Cs.GetProperties()[3]} . * TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}` WITH GRANT OPTION");
                sql.Add($"GRANT ALL PRIVILEGES ON mysql . * TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
            }
            if (type == "curator")
            {
                sql.Add($"GRANT ALL PRIVILEGES ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{user}_template` TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"GRANT ALL PRIVILEGES ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{user}_content` TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"GRANT ALL PRIVILEGES ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{user}_tempcont` TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"GRANT SELECT ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{root}_template` TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"GRANT SELECT ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{root}_content` TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"GRANT SELECT ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{root}_tempcont` TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"GRANT SELECT ON {DataAccessMySQL.Cs.GetProperties()[3]} . `settings` TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"GRANT SELECT ON mysql . * TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
            }
            if (type == "client")
            {
                List<GroupModel> groups = await ReadGroups();
                foreach (GroupModel group in groups)
                {
                    sql.Add($"GRANT SELECT ON {group.Name}_group TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                }
                sql.Add($"GRANT SELECT ON {DataAccessMySQL.Cs.GetProperties()[3]} . `settings` TO `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
            }
            sql.Add($"FLUSH PRIVILEGES");
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task CreateUserGroupPriviliges(IEnumerable<string> selectedGroupNames, UserModel user)
        {
            List<string> userAssignedGroups = await ReadUserGroupPrivileges(user);

            List<string> sql = new();
            foreach (string groupName in userAssignedGroups.Where(x => !selectedGroupNames.Contains(x)))
            {
                sql.Add($"REVOKE SELECT, INSERT, UPDATE, DELETE ON {groupName}_group FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
            }
            foreach (string groupName in selectedGroupNames.Where(x => !userAssignedGroups.Contains(x)))
            {
                sql.Add($"GRANT SELECT, INSERT, UPDATE, DELETE  ON {groupName}_group TO `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
            }
            if (sql.Count() > 0)
            {
                sql.Add($"FLUSH PRIVILEGES");
                await DataAccessMySQL.SaveData(sql, new { });
            }
        }
        public async Task DeletePrivileges(UserModel user, string root)
        {
            List<string> sql = new();
            if (user.Type == "admin")
            {
                sql.Add($"REVOKE ALL PRIVILEGES ON {DataAccessMySQL.Cs.GetProperties()[3]} . * FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"REVOKE GRANT OPTION ON {DataAccessMySQL.Cs.GetProperties()[3]} . * FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"REVOKE ALL PRIVILEGES ON mysql . * FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
            }
            if (user.Type == "curator")
            {
                sql.Add($"REVOKE ALL PRIVILEGES ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{user.Name}_template` FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"REVOKE ALL PRIVILEGES ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{user.Name}_content` FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"REVOKE ALL PRIVILEGES ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{user.Name}_tempcont` FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"REVOKE SELECT ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{root}_template` FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"REVOKE SELECT ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{root}_content` FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"REVOKE SELECT ON {DataAccessMySQL.Cs.GetProperties()[3]} . `{root}_tempcont` FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"REVOKE SELECT ON {DataAccessMySQL.Cs.GetProperties()[3]} . `settings` FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                sql.Add($"REVOKE SELECT ON mysql . * FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
            }
            if (user.Type == "client")
            {
                List<GroupModel> groups = await ReadGroups();
                foreach (GroupModel group in groups)
                {
                    sql.Add($"REVOKE SELECT ON {group.Name}_group FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                }
                sql.Add($"REVOKE SELECT ON {DataAccessMySQL.Cs.GetProperties()[3]} . `settings` FROM `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
            }
            sql.Add($"FLUSH PRIVILEGES");
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task<List<string>> ReadPrivileges(string user)
        {
            return await DataAccessMySQL.LoadData<string>($"SHOW GRANTS FOR `{user}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
        }
        public async Task<List<string>> ReadPrivileges(UserModel user)
        {
            return await ReadPrivileges(user.Name);
        }
        public async Task<List<string>> ReadUserGroupPrivileges(string user)
        {
            List<string> privileges = await ReadPrivileges(user);
            List<string> groupNames = new();
            foreach (string s in privileges)
            {
                if (s.StartsWith("GRANT SELECT, INSERT, UPDATE, DELETE") && s.Contains("_group"))
                {
                    string groupName = s.Split("`").Where(x => x.Contains("_group")).First().Replace("_group", "");
                    groupNames.Add(groupName);
                }
            }
            return groupNames;
        }
        public async Task<List<string>> ReadUserGroupPrivileges(UserModel user)
        {
            return await ReadUserGroupPrivileges(user.Name);
        }
        public async Task UpdatePrivileges(UserModel user, string type, string root)
        {
            await DeletePrivileges(user, root);
            await CreatePrivileges(user.Name, type, root);
        }
        public async Task UpdateClientsPrivileges(IEnumerable<UserModel> clients, List<GroupModel> groups)
        {
            if (clients.Count() > 0)
            {
                List<string> sql = new();
                foreach (UserModel client in clients)
                {
                    foreach (GroupModel group in groups)
                    {
                        sql.Add($"GRANT SELECT ON {group.Name}_group TO `{client.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`");
                    }
                }
                sql.Add($"FLUSH PRIVILEGES");
                await DataAccessMySQL.SaveData(sql, new { });
            }
        }

        /*SETTINGS*/
        public async Task<RepoSettings> ReadRepoSettings()
        {
            return (await DataAccessMySQL.LoadData<RepoSettings>($"SELECT RootName, Color FROM settings"))[0];
        }
        public async Task UpdateRootName(string rootName)
        {
            string[] sql =
            {
                $"CREATE TABLE IF NOT EXISTS `settings`(`Id` int(11) NOT NULL AUTO_INCREMENT,`RootName` VARCHAR(50) NOT NULL,`Color` TEXT, PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=latin1",
                $"INSERT IGNORE INTO settings VALUES(1, null, null)",
                $"UPDATE settings SET RootName = '{rootName}'"
            };
            await DataAccessMySQL.SaveData(sql, new { });
        }

        /*TABLES*/
        public async Task CreateTables(string user, string root)
        {
            string[] sql =
            {
                $"CREATE TABLE IF NOT EXISTS `{user}_template`(`Id` int(11) NOT NULL AUTO_INCREMENT,`Name` VARCHAR(50) NOT NULL, PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=latin1",
                $"CREATE TABLE IF NOT EXISTS `{user}_content`(`Id` int(11) NOT NULL AUTO_INCREMENT,`Type` VARCHAR(50) NOT NULL,`Name` VARCHAR(50) NOT NULL,`Publisher` TEXT,`Version` TEXT,`Command` TEXT,`Url` TEXT,`Instructions` TEXT, PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=latin1",
                $"CREATE TABLE IF NOT EXISTS `{user}_tempcont`(`Id` int(11) NOT NULL AUTO_INCREMENT,`TemplateId` int(11) NOT NULL, `UserContentId` int(11), `RootContentId` int(11), PRIMARY KEY (`Id`), FOREIGN KEY (`TemplateId`) REFERENCES `{user}_template`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE, FOREIGN KEY (`UserContentId`) REFERENCES `{user}_content`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE, FOREIGN KEY (`RootContentId`) REFERENCES `{root}_content`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE) ENGINE=InnoDB DEFAULT CHARSET=latin1"
            };
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task CreateTables(UserModel user, string root)
        {
            await CreateTables(user.Name, root);
        }
        public async Task DeleteTables(UserModel user)
        {
            string[] sql =
            {
                $"DROP TABLE {user.Name}_tempcont",
                $"DROP TABLE {user.Name}_template, {user.Name}_content"
            };
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task<List<string>> FindTables(string match)
        {
            return await DataAccessMySQL.LoadData<string>($"SHOW TABLES LIKE '{match}'");
        }
        public async Task UpdateTables(UserModel user, string name)
        {
            string sql = $"RENAME TABLE {user.Name}_template TO {name}_template, {user.Name}_content TO {name}_content, {user.Name}_tempcont TO {name}_tempcont";
            await DataAccessMySQL.SaveData(sql, new { });
        }

        /*TEMPLATES*/
        public async Task AddContentToTemplate(List<ContentModel> contentList, TemplateModel template)
        {
            List<string> sql = new();
            foreach (ContentModel content in contentList)
            {
                if (content.Type == "download" && content.User == template.User)
                {
                    sql.Add($"INSERT INTO {template.User}_tempcont VALUES(null, {template.Id}, {content.Id}, null)");
                }
                else if (content.Type == "download" && content.User != template.User)
                {
                    sql.Add($"INSERT INTO {template.User}_tempcont VALUES(null, {template.Id}, null, {content.Id})");
                }
                if (content.Type == "winget")
                {
                    string s = $"INSERT INTO {template.User}_content VALUES(null, '{content.Type}', '{content.Name}', '{content.Publisher}', '{content.Version}', '{content.Command}', null, '{content.Instructions}')";
                    sql.Add(s.Replace("''", "null"));
                    sql.Add($"INSERT INTO {template.User}_tempcont VALUES(null, {template.Id}, LAST_INSERT_ID(), null)");
                }
            }
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task AddUserDownloadToTemplate(ContentModel content, TemplateModel template)
        {
            string sql = $"INSERT INTO {template.User}_tempcont VALUES(null, @TemplateId, @UserContentId, null)";
            await DataAccessMySQL.SaveData(sql, new { TemplateId = template.Id, UserContentId = content.Id });
        }
        public async Task AddRootDownloadToTemplate(ContentModel content, TemplateModel template)
        {
            string sql = $"INSERT INTO {template.User}_tempcont VALUES(null, @TemplateId, null, @RootContentId)";
            await DataAccessMySQL.SaveData(sql, new { TemplateId = template.Id, RootContentId = content.Id });
        }
        public async Task AddWingetToTemplate(ContentModel content, TemplateModel template)
        {
            await CreateContent(content);
            string sql = $"INSERT INTO {template.User}_tempcont VALUES(null, @TemplateId, LAST_INSERT_ID(), null)";
            await DataAccessMySQL.SaveData(sql, new { TemplateId = template.Id });
        }
        public async Task CreateTemplate(string name, string user)
        {
            string sql = $"INSERT INTO {user}_template VALUES(null, @Name)";
            await DataAccessMySQL.SaveData(sql, new { Name = name });
        }
        public async Task DeleteTemplate(TemplateModel template)
        {
            await DeleteContent(template.Content.FindAll(x => x.Type == "winget"));
            string sql = $"DELETE FROM {template.User}_template WHERE Id = @Id";
            await DataAccessMySQL.SaveData(sql, new { Id = template.Id });
        }
        public async Task<List<TemplateModel>> ReadTemplates(string table, string root)
        {
            List<TemplateModel> templates = await DataAccessMySQL.LoadData<TemplateModel>($"SELECT Id, Name, '{table.Remove(table.Length - 9)}' AS User FROM {table}");
            foreach (TemplateModel template in templates)
            {
                template.Content = await ReadContent(template, root);
            }
            return templates;
        }
        public async Task<List<TemplateModel>> ReadTemplates(List<string> tables, string root)
        {
            List<TemplateModel> templates = new();
            foreach (string table in tables)
            {
                templates.AddRange(await ReadTemplates(table, root));
            }
            return templates;
        }
        public async Task RemoveContentFromTemplate(ContentModel content, TemplateModel template)
        {
            if (content.Type == "winget")
            {
                await DeleteContent(content);
            }
            else
            {
                if (content.User == template.User)
                {
                    string sql = $"DELETE FROM {template.User}_tempcont WHERE TemplateId = @TemplateId AND UserContentId = @ContentId";
                    await DataAccessMySQL.SaveData(sql, new { TemplateId = template.Id, ContentId = content.Id });
                }
                else
                {
                    string sql = $"DELETE FROM {template.User}_tempcont WHERE TemplateId = @TemplateId AND RootContentId = @ContentId";
                    await DataAccessMySQL.SaveData(sql, new { TemplateId = template.Id, ContentId = content.Id });
                }
            }
        }
        public async Task RemoveContentFromTemplate(List<ContentModel> contentList, TemplateModel template)
        {
            List<string> sql = new();
            foreach (ContentModel content in contentList)
            {
                if (content.Type == "download" && content.User == template.User)
                {
                    sql.Add($"DELETE FROM {template.User}_tempcont WHERE TemplateId = {template.Id} AND UserContentId = {content.Id}");
                }
                else if (content.Type == "download" && content.User != template.User)
                {
                    sql.Add($"DELETE FROM {template.User}_tempcont WHERE TemplateId = {template.Id} AND RootContentId = {content.Id}");
                }
                else if (content.Type == "winget")
                {
                    sql.Add($"DELETE FROM {content.User}_content WHERE Id = {content.Id}");
                }
            }
            await DataAccessMySQL.SaveData(sql, new { });
        }
        public async Task UpdateTemplate(TemplateModel template, string name)
        {
            string sql = $"UPDATE {template.User}_template SET Name = @Name WHERE Id = @Id";
            await DataAccessMySQL.SaveData(sql, new { Name = name, Id = template.Id });
        }

        /*USERS*/
        public async Task CreateUser(string name, string password, string type, string root)
        {
            string sql = $"CREATE USER IF NOT EXISTS '{name}'@'{DataAccessMySQL.Cs.GetProperties()[0]}' IDENTIFIED BY '{password}'";
            await DataAccessMySQL.SaveData(sql, new { });
            if (type != "client")
                await CreateTables(name, root);
            await CreatePrivileges(name, type, root);
        }
        public async Task DeleteUser(UserModel user)
        {
            string sql = $"DROP USER `{user.Name}`@`{DataAccessMySQL.Cs.GetProperties()[0]}`";
            await DataAccessMySQL.SaveData(sql, new { });
            await DeleteTables(user);
        }
        public async Task<UserModel> FindUser(string match)
        {
            return (await DataAccessMySQL.LoadData<UserModel>($"SELECT DISTINCT User AS Name, Password FROM mysql.user WHERE User = '{match}'"))[0];
        }
        public async Task<List<UserModel>> ReadUsers()
        {
            return await DataAccessMySQL.LoadData<UserModel>($"SELECT DISTINCT User AS Name, Password FROM mysql.user");
        }
        public async Task UpdateUser(UserModel user, string name, string password, string type, string root)
        {
            if (user.Name != name)
            {
                string sql = $"RENAME USER '{user.Name}'@'{DataAccessMySQL.Cs.GetProperties()[0]}' TO '{name}'@'{DataAccessMySQL.Cs.GetProperties()[0]}'";
                await DataAccessMySQL.SaveData(sql, new { });
                await UpdateTables(user, name);
            }
            if (password is not null && user.Password != password)
            {
                string sql = $"ALTER USER '{name}'@'{DataAccessMySQL.Cs.GetProperties()[0]}' IDENTIFIED BY '{password}'";
                await DataAccessMySQL.SaveData(sql, new { });
            }
            if (user.Type != type)
            {
                if (type == "client")
                    await DeleteTables(user);
                if (user.Type == "client")
                    await CreateTables(user, root);
                await UpdatePrivileges(user, type, root);
            }
        }

        /*WINGETS*/
        public async Task<List<ContentModel>> ReadWingets()
        {
            return await DataAccessSQLite.LoadData<ContentModel>("SELECT id, 'winget' as type, name, publisher FROM (SELECT ids.id, versions.version, names.name, norm_publishers.norm_publisher AS publisher FROM manifest JOIN ids on ids.rowid = manifest.id JOIN names ON names.rowid = manifest.name JOIN norm_publishers_map ON norm_publishers_map.manifest = manifest.rowid JOIN norm_publishers ON norm_publishers.rowid = norm_publishers_map.norm_publisher JOIN versions ON versions.rowid = manifest.version ORDER BY versions.version DESC) GROUP BY id ORDER BY name");
        }
        public async Task<List<string>> ReadWingetVersions(ContentModel winget)
        {
            if (winget.Command is not null)
                return await DataAccessSQLite.LoadData<string>($"SELECT versions.version FROM manifest JOIN ids on ids.rowid = manifest.id JOIN versions ON versions.rowid = manifest.version WHERE ids.id = '{winget.Command}' ORDER BY versions.version DESC");
            else
                return await DataAccessSQLite.LoadData<string>($"SELECT versions.version FROM manifest JOIN ids on ids.rowid = manifest.id JOIN versions ON versions.rowid = manifest.version WHERE ids.id = '{winget.Id}' ORDER BY versions.version DESC");
        }


    }
}
