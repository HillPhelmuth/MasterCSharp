using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MasterSharpOpen.Shared;
using MasterSharpOpen.Shared.CodeModels;
using MasterSharpOpen.Shared.StaticAuth;
using MasterSharpOpen.Shared.StaticAuth.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using VideoModels = MasterSharpOpen.Shared.VideoModels;

namespace MasterSharpOpen.Client.Pages
{
    public partial class Index
    {
        [Inject]
        public AppStateService AppStateService { get; set; }
        [Inject]
        protected PublicClient PublicClient { get; set; }
        [Inject]
        private ICustomAuthenticationStateProvider AuthProvider { get; set; }
        private int tabIndex = 0;
        private bool isPageReady;
        

        protected override async Task OnInitializedAsync()
        {
            var authInfo = await AuthProvider.GetAuthenticationStateAsync();
            if (authInfo?.User?.Identity?.IsAuthenticated ?? false)
            {
                var userName = authInfo.User.Identity.Name;
                Console.WriteLine($"user {userName} found");
                var currentUser = await PublicClient.GetOrAddUserAppData(userName);
                Console.WriteLine($"retrieved user profile for {currentUser.Name}");
                AppStateService.UpdateUserAppData(currentUser);
            }
           
            AppStateService.OnTabChange += HandleTabNavigation;
            isPageReady = true;
            StateHasChanged();
        }

        protected void HandleTabNavigation(int tab)
        {
            if (tab < 0 || tab > 4)
                return;
            tabIndex = tab;
            StateHasChanged();
        }

       
    }
}
