using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Shared.ArenaChallenge
{
    public class ArenaService
    {
        public ArenaService()
        {
            ActiveArenas = new List<Arena>();
        }
        public IEnumerable<Arena> OpenArenas => ActiveArenas?.Where(x => !x.IsFull);
        public List<Arena> ActiveArenas { get; private set; } 
        
        //public event Func<string, string, Task> OnArenaCreate;
        //public event Func<string, Task> OnArenaJoined;
        public event Action OnArenaRemoved;
        public event Action OnArenasChanged;
        public event Func<string, Task> OnArenaCompleted; 
        public event Func<Task> OnArenasUpdate; 

        public void UpdateArenas(List<Arena> arenas)
        {
            ActiveArenas = arenas;
            NotifyArenaUpdate();
        }
        public void CreateArena(Arena arena)
        {
            ActiveArenas ??= new List<Arena>();
            ActiveArenas.Add(arena);
            //NotifyCreateArena(arena.Name, arena.CurrentChallenge?.Name);
            NotifyArenaChanged();
        }

        public void ArenaComplete(string arenaName) => NotifyArenaComplete(arenaName);

        public void JoinArena(string arenaName, string userName)
        {
            if (ActiveArenas == null) return;
            var arenaToJoin = ActiveArenas.FirstOrDefault(x => x.Name == arenaName);
            //if (arenaToJoin == null || !arenaToJoin.IsFull)
            //{
            //    CreateArena(arenaName, userName);
            //    return;
            //}
            arenaToJoin.Opponent = userName;
            //NotifyJoinArena(arenaName);
            NotifyArenaChanged();
        }

        public void RemoveArena(Arena arena)
        {
            if (ActiveArenas == null) return;
            ActiveArenas.RemoveAll(x => x.Name == arena.Name && x.Creator == arena.Creator);
            NotifyArenaRemoved();
            NotifyArenaChanged();
        }
        //private void NotifyCreateArena(string arenaName, string challengeName)
        //{
        //    OnArenaCreate?.Invoke(arenaName, challengeName);
        //}

        //private void NotifyJoinArena(string arenaName)
        //{
        //    OnArenaJoined?.Invoke(arenaName);
        //}
        private void NotifyArenaUpdate() => OnArenasUpdate?.Invoke();
        private void NotifyArenaRemoved() => OnArenaRemoved?.Invoke();
        private void NotifyArenaChanged() => OnArenasChanged?.Invoke();
        private void NotifyArenaComplete(string name) => OnArenaCompleted?.Invoke(name);
    }
}
