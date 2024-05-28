using UnityEngine;
using FPLibrary;
using UFE3D;

[System.Serializable]
public class SerializedAnimationData
{
    public string id;
    public AnimationClip clip;
    public CustomHitBoxesInfo customHitBoxDefinition;
    public Fix64 length;
    public bool bakeSpeed = false;
    public HitBoxDefinitionType hitBoxDefinitionType;
}