using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
[ExecuteInEditMode]

public class MapItem : Base
{
    [SerializeField]
    public int score;

    IEnumerable<FieldInfo> GetTypes()
    {
        foreach (var f in this.GetType().GetFields())
            if (f.IsPublic && f.GetCustomAttributes(typeof(SerializeField),true).Length > 0)
                yield return f;
    }

    public virtual string title() { return ""; }
    protected override void Awake()
    {

        //base.Awake();
    }
    void Start()
    {
        name = this.GetType() + "";
        foreach (var f in GetTypes())
        {
            name += "," + f.GetValue(this);
        }
    }
    public virtual void CheckOut()
    {

    }
    public void Parse()
    {
        int i = 1;
        string[] s = name.Split(',');
        foreach (var f in GetTypes())
        {
            //print("typeof:" + f + " value" + s[i]);
            try
            {
                f.SetValue(this, s[i]);
            }catch{ }
            try
            {
                f.SetValue(this, int.Parse(s[i]));
            }
            catch { }
            i++;
        }
    }

}
