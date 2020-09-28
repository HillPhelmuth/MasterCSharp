using System;
using System.ComponentModel;
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
        private int tabIndex = 0;
        private bool isPageReady;

        protected override Task OnInitializedAsync()
        {
            AppStateService.PropertyChanged += HandleTabNavigation;
            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var authInfo = await AuthProvider.GetAuthenticationStateAsync();
                if (authInfo?.User?.Identity?.IsAuthenticated ?? false)
                {
                    var userName = authInfo.User.Identity.Name;
                    Console.WriteLine($"user {userName} found");
                    var currentUser = await PublicClient.GetOrAddUserAppData(userName);
                    AppStateService.UpdateUserAppData(currentUser);
                    isPageReady = true;
                    await InvokeAsync(StateHasChanged);
                }
            }
            isPageReady = true;

            await base.OnAfterRenderAsync(firstRender);
        }

        private void SetTab(int tab)
        {
            if (!isPageReady) return;
            AppStateService.TabIndex = tab;
        }
        protected void HandleTabNavigation(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "TabIndex") return;
            tabIndex = AppStateService.TabIndex;
            StateHasChanged();
        }
    }
}
