using System.Collections.Generic;
using UnityEngine;

public class SkillBook : MonoBehaviour
{
    public List<Skill> skillsSet = new List<Skill>();
    void Awake()
    {
        if (gameObject.name == "Striker(Clone)")
        {
            skillsSet.Add(new CarnageSlice());
        }

        if (gameObject.name == "Support(Clone)")
        {
        }

        if (gameObject.name == "Protector(Clone)")
        {
        }
    }
}