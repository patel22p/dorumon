// ----------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static bool Contains(this int[] target, int nr)
    {
        if (target == null)
        {
            return false;
        }

        foreach (int entry in target)
        {
            if (entry == nr)
            {
                return true;
            }
        }

        return false;
    }

    // compares the square magniture of target - second to given float value
    public static bool AlmostEquals(this Vector3 target, Vector3 second, float sqrMagniturePrecision)
    {
        return (target - second).sqrMagnitude < sqrMagniturePrecision;
    }

    // compares the angle between target and second to given float value
    public static bool AlmostEquals(this Quaternion target, Quaternion second, float maxAngle)
    {
        return Quaternion.Dot(target, second) < maxAngle;
    }

    public static void Merge(this IDictionary target, IDictionary addHash)
    {
        if (addHash == null)
        {
            return;
        }

        foreach (object key in addHash.Keys)
        {
            target[key] = addHash[key];
        }
    }

    /// <summary>
    /// Does not remove keys from target (so non-string keys CAN be in target if they were before)
    /// </summary>
    /// <param name="target">Is modified with addHash: new keys are copied, existing will get values from addHash</param>
    /// <param name="addHash"></param>
    public static void MergeStringKeys(this IDictionary target, IDictionary addHash)
    {
        if (addHash == null)
        {
            return;
        }

        foreach (object key in addHash.Keys)
        {
            if (key is string)
            {
                target[key] = addHash[key];
            }
        }
    }

    // does not recurse (!) into hashes that might be values in the root-hash
    public static Hashtable StripToStringKeys(this IDictionary original)
    {
        Hashtable target = new Hashtable();
        foreach (DictionaryEntry pair in original)
        {
            if (pair.Key is string)
            {
                target[pair.Key] = pair.Value;
            }
        }

        return target;
    }
}
