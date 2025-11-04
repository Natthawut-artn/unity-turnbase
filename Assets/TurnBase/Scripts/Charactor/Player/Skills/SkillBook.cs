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
            skillsSet.Add(new Shunpo());
        }

        if (gameObject.name == "Support(Clone)")
        {
            skillsSet.Add(new Frostbite());
        }

        if (gameObject.name == "Protector(Clone)")
        {
            skillsSet.Add(new SpiritShield());
            skillsSet.Add(new DivineRevival());
        }
    }
}