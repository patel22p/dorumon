using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

 /*
  * TODO:
  * 
  * Encrypt / decrypt
  * Save game validation check
  * 
  * */
public class DataHandler
{

    #region DataHandler variables
    // Folder reading
    string profilePath = @"\Profiles\";
    FileInfo[] files;


    // XML writing and reading
    XmlTextReader myReader;
    XmlTextWriter myWriter;
    XmlSerializer mySerialize;

    // Profile Lister
    #endregion


    #region Construcor
    // Constructor
    public DataHandler()
    {
        profilePath = Directory.GetCurrentDirectory() + profilePath;
        // Constructor data.
    }
    #endregion


    #region Valide profile Folder
    // To check errors in saves and so that they are not corrupt. If so, they will be deleted.
    public void ValidateProfiles()
    {
        // Check if the folder exists.. if not, it will be created
        if (!Directory.Exists(profilePath))
        {
            Directory.CreateDirectory(profilePath);
        }
    }
    #endregion


    #region Return Profile List in string[]
    // Count the files in profile diectory
    public int CountProfiles()
    {
        DirectoryInfo getDir = new DirectoryInfo(profilePath);
        files = getDir.GetFiles("*.dat");

        return files.Length;
    }
    #endregion


    #region Check Profile exists
    // To check existing profiles in the profile folder.
    public bool ProfileExists(string profileName)
    {
        // Iterate through profile files
        mySerialize = new XmlSerializer(typeof(Profile));
        DirectoryInfo getDir = new DirectoryInfo(profilePath);
        files = getDir.GetFiles("*.dat");
        if (files.Length>0)
        {
            foreach (FileInfo f in files)
            {
                myReader = new XmlTextReader(profilePath + f.Name);
                Profile p = (Profile)mySerialize.Deserialize(myReader);
                if ((profileName).ToUpper() == p.ProfileName.ToUpper())
                {
                    myReader.Close();
                    return true;
                }
                myReader.Close();
            }
        }

        return false;
    }
    #endregion

    #region New Profile
    // Create a new XML profile file
    public bool NewProfileXML(string profileName)
    {
        bool writeSet = false;
        // ITerate through files and create when a empty slot occurs
        DirectoryInfo getDir = new DirectoryInfo(profilePath);
        files = getDir.GetFiles("*.dat");

        mySerialize = new XmlSerializer(typeof(Profile));

        int i = 0;
        // Open the XML file, if it doesnt exist.. one will be created.
        if (files.Length > 0)
        {
            foreach (FileInfo f in files)
            {
                string comparedSlot = f.Name.Remove(0, 4); // Remove "save" from the string
                comparedSlot = comparedSlot.Remove(comparedSlot.Length - 4); // remove .dat from the string and not it is ready to be compared.

                if (!string.Equals(comparedSlot, i.ToString()))
                {
                    writeSet = true;
                    Profile p = new Profile(profileName, i);
                    myWriter = new XmlTextWriter(profilePath + "save" + i + ".dat", null);
                    mySerialize.Serialize(myWriter, p);

                    // Update the gamedata also
                    GameData myGame = (GameData)GameObject.Find("GAMEDATA_SINGLETON").GetComponent<GameData>();
                    myGame.SetProfile(p);
                    break;
                }
                i++;
            }
        }

        if (!writeSet)
        {
            // Create the file.
            Profile p = new Profile(profileName, i);
            myWriter = new XmlTextWriter(profilePath + "save" + i + ".dat", null);
            mySerialize.Serialize(myWriter, p);

            // Update the gamedata also
            GameData myGame = (GameData)GameObject.Find("GAMEDATA_SINGLETON").GetComponent<GameData>();
            myGame.SetProfile(p);
        }

        // Close the XML writer
        myWriter.Close();
        return true;
    }
    #endregion


    #region Delete Profile
    // Delete desired XML profile file
    public bool DeleteProfile(int slot)
    {
        DirectoryInfo getDir = new DirectoryInfo(profilePath);
        files = getDir.GetFiles("*.dat");
        foreach (FileInfo f in files)
        {
            if (("save" + slot + ".dat").ToUpper() == f.Name.ToUpper())
            {
                f.Delete();
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Save Profile
    public bool SaveProfile(Profile profile)
    {
        Profile p = profile;
        mySerialize = new XmlSerializer(typeof(Profile));
        myWriter = new XmlTextWriter(profilePath + "save"+ profile.ProfileSlot + ".dat", null);

        mySerialize.Serialize(myWriter, p);

        myWriter.Close();
        return true;
    }

    #endregion

    #region ReturnProfileList as typeof(Profile)
    public Profile[] ReturnProfiles()
    {
        // Iterate through profile files
        Profile[] profiles;
        mySerialize = new XmlSerializer(typeof(Profile));
        DirectoryInfo getDir = new DirectoryInfo(profilePath);
        files = getDir.GetFiles("*.dat");

        profiles = new Profile[files.Length];

        if (files.Length > 0)
        {
            int i = 0;
            foreach (FileInfo f in files)
            {
                myReader = new XmlTextReader(profilePath + f.Name);
                profiles[i] = (Profile)mySerialize.Deserialize(myReader);
                
                myReader.Close();
                i++;
            }
        }

        return profiles;
    }
    #endregion
}
