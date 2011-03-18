using System;
using UnityEngine;

public class Invite : bs
{
    void Start()
    {
        _InvateWindow.Show(this);        
        Application.ExternalEval(@"FB.getLoginStatus(function(response) {
  onStatus(response); // once on page load
  FB.Event.subscribe('auth.statusChange', onStatus); ");

    }
    void Action(InvateWindowEnum a)
    {
        if (a == InvateWindowEnum.Close || a == InvateWindowEnum.Skip)
            Application.LoadLevel(Application.loadedLevel + 1);

        if (a == InvateWindowEnum.Invite)
        {
            Application.ExternalEval(@"FB.ui({
    method: 'apprequests',
    message: '3D Game Ropector http://www.youtube.com/watch?v=LnYmmddTNrs',
  }, null);");
        }
    }
}