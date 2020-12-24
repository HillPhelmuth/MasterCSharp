using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorApp.Shared;
using BlazorApp.Shared.CodeServices;
using BlazorApp.Shared.StaticAuth.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages
{
    public partial class Index
    {
        [Inject]
        public AppState AppState { get; set; }
        [Inject]
        protected PublicClient PublicClient { get; set; }
        [Inject]
        private ICustomAuthenticationStateProvider AuthProvider { get; set; }

        private bool isPageReady;

        protected override Task OnInitializedAsync()
        {
            AppState.PropertyChanged += HandlePropertyChanged;
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
                    AppState.UpdateUserAppData(currentUser);
                }
                isPageReady = true;
                await InvokeAsync(StateHasChanged);
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        private void SetTab(int tab)
        {
            if (!isPageReady) return;
            AppState.TabIndex = tab;
        }
        protected void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "TabIndex") return;
            //tabIndex = AppStateService.TabIndex;
            StateHasChanged();
        }
    }
}
