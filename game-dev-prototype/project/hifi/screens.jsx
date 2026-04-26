/* ====== Hi-fi: Game Over + Hotseat Leader Select ====== */

// ===== Game Over: 4-up scoreboard =====
const HfGameOver = ({ winner = 'kazakh', onRematch, onMenu }) => {
  const [showAnnotations, setShowAnnotations] = React.useState(false);
  React.useEffect(() => {
    const h = (e) => { if (e.key === 'a' || e.key === 'A') setShowAnnotations(v => !v); };
    window.addEventListener('keydown', h);
    return () => window.removeEventListener('keydown', h);
  }, []);

  const standings = [
    { f: 'kazakh', t: 22, w: 35, mission: true },
    { f: 'timurid', t: 18, w: 28, mission: false },
    { f: 'slavic', t: 14, w: 20, mission: false },
    { f: 'ming', t: 10, w: 12, mission: false },
  ];

  return (
    <div className="t-wood-dark" style={{ width: 1920, height: 1080, position: 'relative', overflow: 'hidden' }}>
      <div style={{ position: 'absolute', inset: 0, background: 'rgba(0,0,0,0.5)' }} />
      <Embers count={15} />
      <Vignette />

      <div className="anim-fade" style={{ position: 'absolute', left: 60, right: 60, top: 50, bottom: 50, zIndex: 2 }}>
        <Panel tex="t-leather" style={{ padding: 30, height: '100%',
          boxShadow: '0 12px 48px rgba(0,0,0,0.7)' }}>
          <Orn variant="weave" />
          <div style={{ textAlign: 'center', marginTop: 16 }}>
            <div style={{
              fontFamily: 'var(--font-title)', fontSize: 42, fontWeight: 900,
              textShadow: '0 3px 12px rgba(0,0,0,0.6)',
            }}>
              <span className="shimmer-text">The Khural Decides</span>
            </div>
            <div style={{ fontFamily: 'var(--font-body)', fontSize: 16, color: 'var(--gold-dim)', fontStyle: 'italic', marginTop: 4 }}>
              The game is over · final standings
            </div>
          </div>
          <Orn variant="weave" style={{ marginTop: 14 }} />

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 18, marginTop: 28 }}>
            {standings.map((e, i) => {
              const l = LEADERS[e.f];
              const isWinner = e.f === winner;
              return (
                <div key={e.f} className={`anim-fade anim-d${i + 3}`}>
                  <Panel felt={e.f} style={{
                    padding: 16,
                    boxShadow: isWinner
                      ? '0 0 0 2px var(--gold), 0 0 30px var(--gold-glow), 0 6px 24px rgba(0,0,0,0.5)'
                      : '0 4px 16px rgba(0,0,0,0.4)',
                    borderColor: isWinner ? 'var(--gold)' : 'var(--brass-border)',
                  }}>
                    {isWinner && (
                      <div className="anim-pulse" style={{
                        padding: '4px 14px', background: 'rgba(200,168,78,0.2)',
                        border: '1px solid var(--gold)', borderRadius: 4,
                        textAlign: 'center', marginBottom: 10,
                      }}>
                        <HfTitle size={12} style={{ color: 'var(--gold-bright)' }}>★ VICTOR ★</HfTitle>
                      </div>
                    )}
                    {/* Portrait */}
                    <div style={{ width: '100%', height: 180, borderRadius: 4, overflow: 'hidden',
                      border: `2.5px solid ${isWinner ? 'var(--gold)' : 'var(--brass)'}`,
                      boxShadow: isWinner ? '0 0 16px var(--gold-glow)' : '0 2px 8px rgba(0,0,0,0.4)' }}>
                      <img src={l.img} alt={l.name} style={{
                        width: '100%', height: '100%', objectFit: 'cover', objectPosition: 'top center',
                        filter: isWinner ? 'none' : 'brightness(0.75) saturate(0.8)',
                      }} />
                    </div>
                    <HfTitle size={16} style={{ display: 'block', marginTop: 10 }}>{l.name}</HfTitle>
                    <HfLabel>{l.empire}</HfLabel>
                    <Orn style={{ margin: '10px 0' }} />
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr auto', gap: 6, alignItems: 'center' }}>
                      <span style={{ fontFamily: 'var(--font-body)', fontSize: 13, color: 'var(--cream-dim)' }}>Territories</span>
                      <HfTitle size={15}>{e.t}</HfTitle>
                      <span style={{ fontFamily: 'var(--font-body)', fontSize: 13, color: 'var(--cream-dim)' }}>Warriors</span>
                      <HfTitle size={15}>{e.w}</HfTitle>
                      <span style={{ fontFamily: 'var(--font-body)', fontSize: 13, color: 'var(--cream-dim)' }}>Mission</span>
                      <HfTitle size={15} style={{ color: e.mission ? 'var(--gold-bright)' : 'var(--timurid)' }}>
                        {e.mission ? '✓ Complete' : '✗ Failed'}
                      </HfTitle>
                    </div>
                  </Panel>
                </div>
              );
            })}
          </div>

          <div className="anim-fade anim-d7" style={{ display: 'flex', gap: 14, marginTop: 32, justifyContent: 'center' }}>
            <HfBtn felt="kazakh" glow onClick={onRematch}>REMATCH</HfBtn>
            <HfBtn felt="slavic" onClick={onMenu}>MAIN MENU</HfBtn>
          </div>

          {showAnnotations && <>
            <UiSpec text="Panel_GameOver [Panel+VLayout] → VictorBadge+LeaderCard×4+BtnRow" style={{ top: -18, left: 0 }} />
            <SfxCue text="victory_fanfare_orchestral" style={{ top: -18, right: 0 }} />
          </>}
        </Panel>
      </div>
    </div>
  );
};

// ===== Hotseat Leader Select =====
const HfHotseat = ({ onStart }) => {
  const [p1, setP1] = React.useState('kazakh');
  const [p2, setP2] = React.useState('timurid');
  const [hovered, setHovered] = React.useState(null);
  const [showAnnotations, setShowAnnotations] = React.useState(false);

  React.useEffect(() => {
    const h = (e) => { if (e.key === 'a' || e.key === 'A') setShowAnnotations(v => !v); };
    window.addEventListener('keydown', h);
    return () => window.removeEventListener('keydown', h);
  }, []);

  const factions = ['kazakh', 'timurid', 'slavic', 'ming'];

  const renderPicker = (selected, setSelected, otherSelected, playerNum) => (
    <Panel tex="t-leather" style={{ padding: 16, boxShadow: '0 8px 30px rgba(0,0,0,0.5)' }}>
      <HfLabel>player {playerNum}</HfLabel>
      <Orn style={{ margin: '8px 0' }} />
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 12 }}>
        {factions.map(f => {
          const l = LEADERS[f];
          const isSel = selected === f;
          const isTaken = otherSelected === f;
          return (
            <div key={f}
              className="pointer transition"
              onMouseEnter={() => setHovered(f + playerNum)}
              onMouseLeave={() => setHovered(null)}
              onClick={() => !isTaken && setSelected(f)}
              style={{ position: 'relative' }}>
              <Panel felt={f} style={{
                padding: 12, cursor: isTaken ? 'not-allowed' : 'pointer',
                opacity: isTaken ? 0.3 : 1,
                boxShadow: isSel
                  ? '0 0 0 2px var(--gold), 0 0 20px var(--gold-glow), 0 4px 16px rgba(0,0,0,0.4)'
                  : hovered === f + playerNum && !isTaken
                    ? '0 0 0 1px var(--brass), 0 0 12px var(--gold-glow)'
                    : '0 3px 10px rgba(0,0,0,0.3)',
                borderColor: isSel ? 'var(--gold)' : 'var(--brass-border)',
                transition: 'all 0.3s',
              }}>
                <div style={{ width: '100%', height: 130, borderRadius: 4, overflow: 'hidden',
                  border: `2px solid ${isSel ? 'var(--gold)' : `var(--${f})`}` }}>
                  <img src={l.img} alt={l.name} style={{
                    width: '100%', height: '100%', objectFit: 'cover', objectPosition: 'top center',
                    filter: isTaken ? 'grayscale(1) brightness(0.5)' : isSel ? 'none' : 'brightness(0.85)',
                    transition: 'filter 0.3s',
                  }} />
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 8 }}>
                  <div>
                    <HfTitle size={13}>{l.name}</HfTitle>
                    <div><HfLabel>{l.empire}</HfLabel></div>
                  </div>
                  {isSel && (
                    <span className="anim-pulse" style={{
                      padding: '2px 8px', background: 'rgba(200,168,78,0.2)',
                      border: '1px solid var(--gold)', borderRadius: 3,
                      fontFamily: 'var(--font-label)', fontSize: 9, color: 'var(--gold)',
                      textTransform: 'uppercase',
                    }}>selected</span>
                  )}
                  {isTaken && (
                    <span style={{
                      padding: '2px 8px', background: 'rgba(0,0,0,0.4)',
                      border: '1px solid rgba(200,168,78,0.2)', borderRadius: 3,
                      fontFamily: 'var(--font-label)', fontSize: 9, color: 'var(--gold-dim)',
                      textTransform: 'uppercase',
                    }}>taken</span>
                  )}
                </div>
              </Panel>
            </div>
          );
        })}
      </div>
    </Panel>
  );

  return (
    <div className="t-wood-dark" style={{ width: 1920, height: 1080, position: 'relative', overflow: 'hidden' }}>
      <div style={{ position: 'absolute', inset: 0,
        background: `radial-gradient(ellipse at 50% 50%, rgba(200,168,78,0.04) 0%, transparent 60%), var(--bg-dark)` }} />
      <Embers count={10} />
      <Vignette />

      {/* Title */}
      <div className="anim-fade" style={{ position: 'absolute', left: '50%', top: 30, transform: 'translateX(-50%)',
        textAlign: 'center', zIndex: 2 }}>
        <Orn variant="weave" style={{ marginBottom: 10 }} />
        <div style={{
          fontFamily: 'var(--font-title)', fontSize: 36, fontWeight: 900,
          textShadow: '0 3px 12px rgba(0,0,0,0.6)',
        }}>
          <span className="shimmer-text">Choose Your Khans</span>
        </div>
        <div style={{ fontFamily: 'var(--font-body)', fontSize: 15, color: 'var(--gold-dim)', fontStyle: 'italic', marginTop: 6 }}>
          Select a leader for each player · hotseat mode
        </div>
      </div>

      {/* Two pickers + VS */}
      <div className="anim-fade anim-d2" style={{
        position: 'absolute', left: 50, right: 50, top: 140, bottom: 90,
        display: 'grid', gridTemplateColumns: '1fr 90px 1fr', gap: 18, zIndex: 2,
      }}>
        {renderPicker(p1, setP1, p2, 1)}
        <div style={{ display: 'grid', placeItems: 'center' }}>
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontFamily: 'var(--font-title)', fontSize: 40, fontWeight: 900, color: 'var(--cream)' }}>VS</div>
            <Orn style={{ margin: '12px 0', width: 60 }} />
          </div>
        </div>
        {renderPicker(p2, setP2, p1, 2)}
      </div>

      {/* Bottom actions */}
      <div className="anim-fade anim-d5" style={{
        position: 'absolute', left: '50%', bottom: 26, transform: 'translateX(-50%)',
        display: 'flex', gap: 14, zIndex: 2,
      }}>
        <HfBtn onClick={onStart}>← BACK</HfBtn>
        <HfBtn felt="kazakh" glow onClick={() => onStart && onStart(p1, p2)}>BEGIN CAMPAIGN →</HfBtn>
      </div>

      {showAnnotations && <>
        <UiSpec text="Scene_LeaderSelect [Canvas] → Panel_P1Picker + Panel_P2Picker + BtnRow" style={{ position: 'absolute', top: 10, left: 10, zIndex: 100 }} />
        <SfxCue text="ui_select_brass_click" style={{ position: 'absolute', bottom: 10, left: 10, zIndex: 100 }} />
      </>}
    </div>
  );
};

Object.assign(window, { HfGameOver, HfHotseat });
