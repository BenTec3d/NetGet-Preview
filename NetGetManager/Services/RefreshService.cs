using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetGetManager
{
    public class RefreshService
    {
        public event Action AvailableContentDetailsRefreshRequested;
        public event Action AvailableContentTableRefreshRequested;
        public event Action ContentDetailsRefreshRequested;
        public event Action ContentTableRefreshRequested;
        public event Action DownloadDetailsRefreshRequested;
        public event Action DownloadsTableRefreshRequested;
        public event Action NavMenuRefreshRequested;
        public event Action UserDetailsRefreshRequested;
        public event Action UsersTableRefreshRequested;
        public event Action ViewRefreshRequested;

        public void CallRequestRefresh()
        {
            CallNavMenuRefreshRequested();
            CallViewRefreshRequested();
            CallAvailableContentTableRefreshRequested();
            CallDownloadDetailsRefreshRequested();
        }
        public void CallAvailableContentDetailsRefreshRequested()
        {
            AvailableContentDetailsRefreshRequested?.Invoke();
        }
        public void CallAvailableContentTableRefreshRequested()
        {
            AvailableContentTableRefreshRequested?.Invoke();
        }
        public void CallContentDetailsRefreshRequested()
        {
            ContentDetailsRefreshRequested?.Invoke();
        }
        public void CallContentTableRefreshRequested()
        {
            ContentTableRefreshRequested?.Invoke();
        }
        public void CallDownloadDetailsRefreshRequested()
        {
            DownloadDetailsRefreshRequested?.Invoke();
        }
        public void CallDownloadsTableRefreshRequested()
        {
            DownloadsTableRefreshRequested?.Invoke();
        }
        public void CallNavMenuRefreshRequested()
        {
            NavMenuRefreshRequested?.Invoke();
        }
        public void CallUserDetailsRefreshRequested()
        {
            UserDetailsRefreshRequested?.Invoke();
        }
        public void CallUsersTableRefreshRequested()
        {
            UsersTableRefreshRequested?.Invoke();
        }
        public void CallViewRefreshRequested()
        {
            ViewRefreshRequested?.Invoke();
        }
    }
}
