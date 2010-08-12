using UnityEngine;
using System.Collections;
using doru;
using System.Collections.Generic;
using System;


    public class Tower : IPlayer
    {
        public override Vector3 SpawnPoint() { throw new NotImplementedException(); }
        protected override void OnStart()
        {
            if (Network.isServer)
                RPCSetOwner();            
        }
        protected override void OnUpdate()
        {
            title.GetComponent<TextMesh>().text = Life.ToString();
        }
        public override void RPCDie()
        {
            
            _Loader.rpcwrite("team " + Team.ata + " win");
            _Loader.LoadLevelRPC(_Loader.disconnectedLevel);
            
        }
    }
