using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;
using System;


    public class Tower : IPlayer
    {
        public override Vector3 SpawnPoint() { throw new NotImplementedException(); }
        void Start()
        {
            if (Network.isServer)
                RPCSetOwner();            
        }
        protected override void Update()
        {
            
                title.GetComponent<TextMesh>().text = Life.ToString();            
        }
        public override void RPCSetLife(int NwLife)
        {
            base.RPCSetLife(NwLife);
            Player p = _Spawn.players[killedyby];
            if (p.team == Team.def)
            {
                p.RPCSetLife(p.Life - (Life - NwLife));
            }
        }
        public override void RPCDie()
        {
            if (!enabled) return;
            _Loader.rpcwrite("team " + Team.ata + " win");
            this.Hide();
            _Loader.LoadLevelRPC(_Loader.disconnectedLevel);
            Screen.lockCursor = false;
        }
    }
