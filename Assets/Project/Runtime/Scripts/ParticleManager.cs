using UnityEngine;
using UnityEngine.VFX;
using System.Linq;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private float drawInterval;

    private HandProvider _hp;
    private VisualEffect[] _effects;
    private TextManager _tm;
    private ResultsManager _rm;
    private OVRSkeleton _activeHand;
    private bool _drawEffect;
    private float _drawTime;

    private void Start()
    {
        _hp = FindObjectOfType<HandProvider>();
        _effects = GetComponentsInChildren<VisualEffect>();
        _tm = FindObjectOfType<TextManager>();
        _rm = FindObjectOfType<ResultsManager>();
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
        // TODO: have color scale logarithmically across all possibilities...
        var colors = new Color[bonePositions.Length];
        var delta = VocabularyProvider.GetVocabJustLetters().Length - 1 - _rm.GetResults().Keys.ToList().IndexOf(char.ToLower(_tm.GetTargetCharacter()[0]));
        //for (var i = 0; i < _rm.GetResults().Keys.ToList().Count; i++) Debug.Log($"i: {i}, GetResults[i]: {_rm.GetResults().Keys.ToList()[i]}");
        //Debug.Log($"Delta: {_rm.GetResults().Keys.ToArray()[0]}");
        var value = Mathf.Clamp(delta, 0f, 3f) / 3f;
        //Debug.Log($"value: {delta}");
        var color = new Color(255f * value, 255f * (1f - value), 0f, 255f);

        for (var i = 0; i < _effects.Length; i++)
        {
            _effects[i].gameObject.transform.position = bonePositions[i];
            _effects[i].SetVector4("ParticleColor", color);
        }
    }

    public void BeginDrawParticles()
    {
        //_hp.UseRightHand();  // TODO: remover later...
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
