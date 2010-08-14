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
        [RPC]
        public override void RPCSetLife(int NwLife)
        {
            
            Player p = _Spawn.players[killedyby];
            if (p.team != Team.def)
                base.RPCSetLife(NwLife);
        }
        [RPC]
        public override void RPCDie()
        {
            if (!enabled) return;
            _Loader.rpcwrite("team " + "Atackers" + " win");
            this.Hide();
            _Loader.LoadLevelRPC(_Loader.disconnectedLevel);
            Screen.lockCursor = false;
        }
    }
