using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorApp.Shared;
using BlazorApp.Shared.StaticAuth.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages
{
    public partial class Index
    {
        [Inject]
        public AppStateService AppStateService { get; set; }
        [Inject]
        protected PublicClient PublicClient { get; set; }
        [Inject]
        private ICustomAuthenticationStateProvider AuthProvider { get; set; }
        [Inject]
        private HttpClient Http { get; set; }
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
