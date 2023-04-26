using UnityEngine;
using UnityEngine.VFX;
using System.Linq;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private float drawInterval;

    private HandProvider _hp;
    private GestureManager _gm;
    private TextManager _tm;
    private VisualEffect[] _effects;
    private OVRSkeleton _activeHand;
    private bool _drawEffect;
    private float _drawTime;

    private void Start()
    {
        _hp = FindObjectOfType<HandProvider>();
        _gm = FindObjectOfType<GestureManager>();
        _tm = FindObjectOfType<TextManager>();
        _effects = GetComponentsInChildren<VisualEffect>();
        _drawTime = drawInterval;
        _drawEffect = false;

        // Set effect attributes
        foreach (var effect in _effects)
        {
            effect.SetInt("MaxSpawnRate", 10);
            effect.SetFloat("PercentageMaxSpawnRate", 1);
        }
    }

    private void Update()
    {
        _drawTime += Time.deltaTime;
        if (!_drawEffect || _drawTime < drawInterval) return;
        _drawTime = 0;

        // Define positions
        var bonePositions = _activeHand.Bones
            .Skip(19)  // Hand_Tip bones are at the end of OVRSkeleton bone array
            .Select(bone => bone.Transform.position)
            .ToArray();

        // Define color
        var colors = new Color[bonePositions.Length];
        var targetIndex = VocabularyProvider.GetVocabArrayJustLetters().ToList().IndexOf(char.ToLower(_tm.GetTargetCharacter()[0]));
        Debug.Log($"targetIndex: {targetIndex}");
        //var targetIndex = 0;
        var (_, aggregateValues) = _gm.GetMeanAndAggregateResults();
        var delta = _gm.GetBoneDeltaValues(in aggregateValues, in targetIndex).Sum();
        //for (var i = 0; i < deltas.Length; i++) Debug.Log($"i: {i}, deltas[i]: {deltas[i]}");
        //Debug.Log($"Delta: {delta}");
        var value = (Mathf.Clamp(delta, 1.5f, 5f) - 1f) / (5f - 1.5f);
        Debug.Log($"value: {value}");
        var color = new Color(255f * value, 255f * (1f - value), 0f, 255f);

        for (var i = 0; i < _effects.Length; i++)
        {
            //_effects[i].SetVector3("ParticlePosition", bonePositions[i]);
            _effects[i].gameObject.transform.position = bonePositions[i];
            _effects[i].SetVector4("ParticleColor", color);  //colors[i]
        }
    }

    public void BeginDrawParticles()
    {
        _hp.UseRightHand();  // TODO: remover later...
        _activeHand = _hp.GetActiveHand();
        //Debug.Log($"_activeHand: {_activeHand}");

        //for (var i = 0; i < _activeHand.Bones.Count; i++) Debug.Log($"i: {i}, name: {_activeHand.Bones[i].Transform.name}");

        _drawEffect = true;
        _drawTime = drawInterval;

        foreach (var effect in _effects) effect.Play();
    }

    public void EndDrawParticles()
    {
        _drawEffect = false;

        foreach (var effect in _effects) effect.Stop();
    }
}
