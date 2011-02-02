using System;

namespace XamlParser
{
    public class S
    {
        public string template = @"
#pragma warning disable 0169, 0414,649,168
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Base2:MonoBehaviour
{    
    static _name_ ___name_;
    public static _name_ __name_ { get { if (___name_ == null) ___name_ = (_name_)MonoBehaviour.FindObjectOfType(typeof(_name_)); return ___name_; } }
}
public enum _name_Enum { _enums_ }
public class _name_ : WindowBase {
		
_fields_	
    
    
	void Start () {
_start_
	}    
    
    
    bool focusWindow;
    void OnEnable()
    {
        focusWindow = true;
    }
    public override void ResetValues()
    {
        _showfunc_
    }
    public override void OnGUI()
    {		
		GUI.skin = _Loader.Skin;
        
_ongui_
    }
_funcs_

	void Update () {
	
	}
}";
    }
}
