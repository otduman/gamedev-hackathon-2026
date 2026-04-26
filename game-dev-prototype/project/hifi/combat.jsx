/* ====== Hi-fi: Combat Overlay — Animated dice, territory names ====== */

const HfCombat = ({ attacker, defender, onClose, onAttackAgain }) => {
  const [rolling, setRolling] = React.useState(true);
  const [showResult, setShowResult] = React.useState(false);
  const [atkDice, setAtkDice] = React.useState([0, 0, 0]);
  const [defDice, setDefDice] = React.useState([0, 0]);
  const [showAnnotations, setShowAnnotations] = React.useState(false);

  const atkLeader = LEADERS[attacker] || LEADERS.timurid;
  const defLeader = LEADERS[defender] || LEADERS.slavic;

  const rollDice = React.useCallback(() => {
    setRolling(true);
    setShowResult(false);
    const a = [1,2,3].map(() => 1 + Math.floor(Math.random() * 6)).sort((a, b) => b - a);
    const d = [1,2].map(() => 1 + Math.floor(Math.random() * 6)).sort((a, b) => b - a);
    setAtkDice(a);
    setDefDice(d);
    setTimeout(() => { setRolling(false); }, 900);
    setTimeout(() => { setShowResult(true); }, 1400);
  }, []);

  React.useEffect(() => { rollDice(); }, [rollDice]);

  React.useEffect(() => {
    const handler = (e) => { if (e.key === 'a' || e.key === 'A') setShowAnnotations(v => !v); };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, []);

  // Resolve: compare highest dice pairs
  const results = [];
  for (let i = 0; i < Math.min(atkDice.length, defDice.length); i++) {
    results.push(atkDice[i] > defDice[i] ? 'atk' : 'def'); // defender wins ties
  }
  const atkLoss = results.filter(r => r === 'def').length;
  const defLoss = results.filter(r => r === 'atk').length;

  return (
    <div style={{ position: 'absolute', inset: 0, zIndex: 50 }}>
      {/* Backdrop */}
      <div className="anim-fade" style={{ position: 'absolute', inset: 0,
        background: 'rgba(0,0,0,0.6)', backdropFilter: 'blur(2px)' }} />

      {/* Attacker mat — slides from left */}
      <div className="anim-slide-left" style={{
        position: 'absolute', left: 40, top: 160, width: 680, zIndex: 51,
      }}>
        <Panel felt={attacker} style={{
          padding: 22, borderColor: `var(--${attacker})`,
          boxShadow: `0 10px 40px rgba(0,0,0,0.7), 0 0 20px var(--${attacker}-glow)`,
          transform: 'rotate(-1.2deg)',
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{ display: 'flex', gap: 14, alignItems: 'center' }}>
              <Portrait faction={attacker} size={72} glow />
              <div>
                <HfTitle size={22}>{atkLeader.name}</HfTitle>
                <HfChip faction={attacker}>{atkLeader.empire} · attacker</HfChip>
                <div style={{ fontFamily: 'var(--font-body)', fontSize: 14, color: 'var(--gold-dim)', marginTop: 6 }}>
                  attacking from <span style={{ color: 'var(--cream)', fontStyle: 'italic' }}>Samarqand</span>
                </div>
              </div>
            </div>
            <div style={{ textAlign: 'center' }}>
              <HfLabel>warriors</HfLabel>
              <HfTitle size={36}>5</HfTitle>
            </div>
          </div>
          <Orn style={{ margin: '14px 0' }} />
          <HfLabel>up to 3 red wooden dice</HfLabel>
          <div style={{ display: 'flex', gap: 12, marginTop: 10 }}>
            {atkDice.map((v, i) => (
              <HfDie key={i} value={rolling ? '?' : v} type="atk" animate={rolling} delay={i * 0.15} />
            ))}
          </div>

          {showAnnotations && <>
            <UiSpec text="Panel_AttackerMat [Panel] → Portrait+TMP+DiceSlot×3" style={{ top: -18, left: 0 }} />
            <SfxCue text="dice_roll_wood_table" style={{ bottom: -18, left: 0 }} />
          </>}
        </Panel>
      </div>

      {/* Defender mat — slides from right */}
      <div className="anim-slide-right" style={{
        position: 'absolute', right: 40, top: 280, width: 680, zIndex: 51,
      }}>
        <Panel felt={defender} style={{
          padding: 22, borderColor: `var(--${defender})`,
          boxShadow: `0 10px 40px rgba(0,0,0,0.7), 0 0 20px var(--${defender}-glow)`,
          transform: 'rotate(1deg)',
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{ textAlign: 'center' }}>
              <HfLabel>warriors</HfLabel>
              <HfTitle size={36}>3</HfTitle>
            </div>
            <div style={{ display: 'flex', gap: 14, alignItems: 'center' }}>
              <div style={{ textAlign: 'right' }}>
                <HfTitle size={22}>{defLeader.name}</HfTitle>
                <HfChip faction={defender}>{defLeader.empire} · defender</HfChip>
                <div style={{ fontFamily: 'var(--font-body)', fontSize: 14, color: 'var(--gold-dim)', marginTop: 6 }}>
                  defending <span style={{ color: 'var(--cream)', fontStyle: 'italic' }}>Kashgar</span>
                </div>
              </div>
              <Portrait faction={defender} size={72} glow />
            </div>
          </div>
          <Orn style={{ margin: '14px 0' }} />
          <HfLabel>up to 2 stone dice · defender wins ties</HfLabel>
          <div style={{ display: 'flex', gap: 12, marginTop: 10, justifyContent: 'flex-end' }}>
            {defDice.map((v, i) => (
              <HfDie key={i} value={rolling ? '?' : v} type="def" animate={rolling} delay={0.2 + i * 0.15} />
            ))}
          </div>

          {showAnnotations && <UiSpec text="Panel_DefenderMat [Panel] → Portrait+TMP+DiceSlot×2" style={{ top: -18, right: 0 }} />}
        </Panel>
      </div>

      {/* Result banner */}
      {showResult && (
        <div className="anim-slide-up" style={{
          position: 'absolute', left: 60, right: 60, bottom: 40, zIndex: 52,
        }}>
          <Panel tex="t-leather" style={{
            padding: 20,
            display: 'grid', gridTemplateColumns: '1fr auto', gap: 24, alignItems: 'center',
            boxShadow: '0 8px 32px rgba(0,0,0,0.6)',
          }}>
            <div>
              <HfLabel>result</HfLabel>
              <div style={{ marginTop: 6 }}>
                {results.map((r, i) => (
                  <span key={i} style={{ marginRight: 16 }}>
                    <HfTitle size={17}>
                      <span style={{ color: `var(--${attacker})` }}>{atkDice[i]}</span>
                      {' vs '}
                      <span style={{ color: `var(--${defender})` }}>{defDice[i]}</span>
                      {' → '}
                      <span style={{ color: r === 'atk' ? 'var(--gold-bright)' : 'var(--cream-dim)' }}>
                        {r === 'atk' ? 'attacker wins' : 'defender wins'}
                      </span>
                    </HfTitle>
                  </span>
                ))}
              </div>
              <div style={{ fontFamily: 'var(--font-body)', fontSize: 15, color: 'var(--gold-dim)', marginTop: 8 }}>
                Attacker loses {atkLoss} · Defender loses {defLoss}. Defender wins ties.
              </div>
            </div>
            <div style={{ display: 'flex', gap: 10 }}>
              <HfBtn felt={attacker} glow onClick={() => { rollDice(); }}>ATTACK AGAIN</HfBtn>
              <HfBtn onClick={onClose}>STOP</HfBtn>
            </div>

            {showAnnotations && <>
              <UiSpec text="Panel_CombatResult [Panel+TMP×3+Btn×2]" style={{ top: -18, left: 0 }} />
              <SfxCue text="combat_resolve_fanfare" style={{ top: -18, right: 0 }} />
            </>}
          </Panel>
        </div>
      )}
    </div>
  );
};

Object.assign(window, { HfCombat });
