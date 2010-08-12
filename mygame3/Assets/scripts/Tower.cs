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
            if (started || !levelLoaded) return;
            started = true;
            if (Network.isServer)
                RPCSetOwner();            
        }
        protected override void Update()
        {
            if (!started) return;
            title.GetComponent<TextMesh>().text = Life.ToString();
            base.Update();
        }
        public override void RPCDie()
        {
            
            _Loader.rpcwrite("team " + Team.ata + " win");
            _Loader.LoadLevelRPC(_Loader.disconnectedLevel);
            
        }
    }
