using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BattleController : MonoBehaviour
{
    [Header("World")]
    [SerializeField] private BattleTile tileTemplate;
    [SerializeField] private CinemachineTargetGroup tileTargetGroup;

    [Header("Interface")]
    [SerializeField] private SelectedUnitPanel selectedUnitPanel;
    [SerializeField] private SelectedAbilityPanel selectedAbilityPanel;
    [SerializeField] private CinemachineImpulseSource selectImpulse;
    [SerializeField] private Animator endTurnButton;

    [SerializeField] private InputActionReference cycleForward;
    [SerializeField] private InputActionReference cycleBackward;

    [SerializeField] private PlayerTeam playerTeam;
    [SerializeField] private EnemyTeam enemyTeam;

    public IEnumerable<Team> AllTeams
    {
        get
        {
            yield return playerTeam;
            yield return enemyTeam;
        }
    }

    public Map Map = new Map(5, 5);

    private Combatant selectedCombatant;

    public Combatant SelectedCombatant
    {
        get => selectedCombatant;
        set
        {
            if (capturingAbility != null)
            {
                CapturingAbility = null;
            }
            if (selectedCombatant != null)
            {
                selectedUnitPanel.Close();
            }

            selectedCombatant = value;

            if (selectedCombatant != null)
            {
                selectedUnitPanel.Render(selectedCombatant);
                selectImpulse.GenerateImpulse();
            }
        }
    }

    private AbilityHandle capturingAbility;

    public AbilityHandle CapturingAbility
    {
        get => capturingAbility;
        set
        {
            if (capturingAbility != null)
            {
                selectedAbilityPanel.Close();
            }

            capturingAbility = value;

            if (capturingAbility != null)
            {
                selectedUnitPanel.Close();
                selectedAbilityPanel.Render(capturingAbility);
            }
            else if (selectedCombatant != null)
            {
                selectedUnitPanel.Render(selectedCombatant);
            }
        }
    }

    void Start()
    {
        tileTemplate.gameObject.SetActive(true);
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                var clone = Instantiate(tileTemplate, new Vector3(y, 0, x) * 2, tileTemplate.transform.rotation);
                tileTargetGroup.Targets.Add(new CinemachineTargetGroup.Target
                {
                    Object = clone.transform,
                    Radius = 1f,
                    Weight = 1f,
                });
                clone.battleController = this;
                Map[x, y] = clone;
                clone.LogicalPosition = new Vector2Int(x, y);
            }
        }
        tileTemplate.gameObject.SetActive(false);

        foreach (var team in AllTeams)
        {
            foreach (var combatant in team.FieldedCombatants)
            {
                var position = team.GetLogicalFieldingPosition(Map);
                var tile = Map[position.x, position.y];
                tile.occupant = combatant;
                combatant.CurrentTile = tile;
                combatant.transform.position = tile.transform.position;
                combatant.Battle = this;
                combatant.Team = team;
            }
        }
    }

    public IEnumerator RunBattle()
    {
        yield return null;
        Debug.Log("Battle is running");

        while (true)
        {
            var currentTeam = playerTeam;

        nounitselected:
            // No Selected Combatant Mode
            while (true)
            {
                if (SelectedCombatant != null)
                {
                    break;
                }

                if (cycleForward.action.WasPressedThisFrame())
                {
                    SelectedCombatant = currentTeam.FieldedCombatants[0];
                }
                else if (cycleBackward.action.WasPressedThisFrame())
                {
                    SelectedCombatant = currentTeam.FieldedCombatants[^1];
                }
                yield return null;
            }

        unitselected:
            // Selection Loop
            while (true)
            {
                if (SelectedCombatant == null)
                {
                    goto nounitselected;
                }

                if (cycleForward.action.WasPressedThisFrame())
                {
                    // Cycle through the team
                    int currentIndex = currentTeam.FieldedCombatants.IndexOf(SelectedCombatant);
                    int nextIndex = (currentIndex + 1) % currentTeam.FieldedCombatants.Count;
                    SelectedCombatant = currentTeam.FieldedCombatants[nextIndex];
                }
                else if (cycleBackward.action.WasPressedThisFrame())
                {
                    // Cycle through the team
                    int currentIndex = currentTeam.FieldedCombatants.IndexOf(SelectedCombatant);
                    if (currentIndex == 0)
                    {
                        currentIndex = currentTeam.FieldedCombatants.Count - 1;
                    }
                    SelectedCombatant = currentTeam.FieldedCombatants[currentIndex];
                }

                foreach (var abilityHandle in SelectedCombatant.activeAbilities)
                {
                    if (abilityHandle.IsClicked)
                    {
                        abilityHandle.IsClicked = false;
                        CapturingAbility = abilityHandle;
                        EventSystem.current.SetSelectedGameObject(selectedCombatant.CurrentTile.Button.gameObject);
                        goto abilityselected;
                    }
                }

                foreach (var abilityHandle in SelectedCombatant.activeAbilities)
                {
                    if (abilityHandle.IsButtonHovered)
                    {
                        var reticuleBuilder = new ReticuleBuilder();
                        abilityHandle.BuildReticule?.Invoke(reticuleBuilder);

                        RenderReticule(reticuleBuilder);
                        break;
                    }
                }

                yield return null;
            }

        abilityselected:
            // Ability Loop
            while (true)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    CapturingAbility = null;
                    goto unitselected;
                }

                if (CapturingAbility == null)
                {
                    goto unitselected;
                }

                if (CapturingAbility.IsCapturedGameflow)
                {
                    goto abilityplaying;
                }

                BattleTile hoveredTile = null;
                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    hoveredTile = EventSystem.current.currentSelectedGameObject.GetComponent<BattleTile>();
                }
                CapturingAbility.HoveredTile = hoveredTile;

                var reticuleBuilder = new ReticuleBuilder();
                CapturingAbility.BuildPreviewReticule?.Invoke(reticuleBuilder);

                RenderReticule(reticuleBuilder);

                yield return null;
            }

        abilityplaying:
            // Ability Loop
            while (true)
            {
                if (!CapturingAbility.IsCapturedGameflow)
                {
                    CapturingAbility = null;
                    goto unitselected;
                }

                yield return null;
            }
        }

        void RenderReticule(ReticuleBuilder reticuleBuilder)
        {
            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    var tile = Map[x, y];
                    var reticuleTile = reticuleBuilder.Reticule.Find(rt => rt.Tile == tile);
                    if (reticuleTile.Tile != null)
                    {
                        tile.SetReticuleState(reticuleTile.Mode);
                    }
                    else
                    {
                        tile.SetReticuleState(0);
                    }
                }
            }
        }
    }

    internal void OnTileSubmitted(BattleTile battleTile)
    {
        if (battleTile != null)
        {
            if (CapturingAbility != null)
            {
                if (CapturingAbility.IsCapturedGameflow)
                {
                    return;
                }
                StartCoroutine(CapturingAbility.CastCoroutine(CapturingAbility, battleTile));
                if (!CapturingAbility.IsCapturedGameflow)
                {
                    CapturingAbility = null;
                }
            }
            else
            {
                if (playerTeam.FieldedCombatants.Contains(battleTile.occupant))
                {
                    // Setting the property automatically handles the UI and Tile state
                    SelectedCombatant = battleTile.occupant;
                }
            }
        }
    }

    public void OnEndTurnButtonSubmit()
    {
    }
}
