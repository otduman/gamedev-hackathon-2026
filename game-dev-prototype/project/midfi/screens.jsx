/* ====== Mid-fi: Combat A (two mats) + Tooltip A (compact) + Game Over B (scoreboard) + Hotseat A (side-by-side) ====== */

// ===== Combat A: Two physical mats sliding from sides =====
const MfCombatA = () => (
  <div className="tex-wood" style={{ width: 1280, height: 760, position: 'relative', overflow: 'hidden' }}>
    {/* Dim map behind */}
    <div style={{ position: 'absolute', inset: 0,
      background: 'linear-gradient(135deg, #1a2a1a 0%, #2a3a1a 50%, #1a2a1a 100%)', opacity: 0.4 }} />
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(0,0,0,0.45)' }} />

    {/* Attacker mat — left, slight rotation */}
    <div style={{ position: 'absolute', left: 30, top: 120, width: 540, transform: 'rotate(-1.2deg)', zIndex: 1 }}>
      <div className="mf-panel tex-felt felt-timurid" style={{ padding: 18,
        boxShadow: '0 8px 30px rgba(0,0,0,0.6), inset 0 1px 0 rgba(255,255,255,0.03)',
        borderColor: 'var(--timurid)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
            <MfPlaceholder style={{ width: 60, height: 60, border: '2px solid var(--timurid)' }}>ATK</MfPlaceholder>
            <div>
              <div className="mf-title" style={{ fontSize: 18 }}>Tamerlane</div>
              <MfChip faction="timurid">Timurid · attacker</MfChip>
              <div className="mf-body" style={{ fontSize: 12, color: 'var(--gold-dim)', marginTop: 4 }}>
                attacking from <span style={{ color: 'var(--cream)', fontStyle: 'italic' }}>Samarqand</span>
              </div>
            </div>
          </div>
          <div style={{ textAlign: 'center' }}>
            <MfLabel>warriors</MfLabel>
            <div className="mf-title" style={{ fontSize: 30 }}>5</div>
          </div>
        </div>
        <MfOrnament style={{ margin: '12px 0' }} />
        <MfLabel>up to 3 red wooden dice</MfLabel>
        <div style={{ display: 'flex', gap: 10, marginTop: 8 }}>
          <MfDie value={6} type="atk" />
          <MfDie value={4} type="atk" />
          <MfDie value={2} type="atk" />
        </div>
      </div>
    </div>

    {/* Defender mat — right, slight rotation */}
    <div style={{ position: 'absolute', right: 30, top: 220, width: 540, transform: 'rotate(1deg)', zIndex: 1 }}>
      <div className="mf-panel tex-felt" style={{ padding: 18, backgroundColor: 'var(--slavic-bg)',
        boxShadow: '0 8px 30px rgba(0,0,0,0.6), inset 0 1px 0 rgba(255,255,255,0.03)',
        borderColor: 'var(--slavic)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ textAlign: 'center' }}>
            <MfLabel>warriors</MfLabel>
            <div className="mf-title" style={{ fontSize: 30 }}>3</div>
          </div>
          <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
            <div style={{ textAlign: 'right' }}>
              <div className="mf-title" style={{ fontSize: 18 }}>Dmitry Donskoy</div>
              <MfChip faction="slavic">Slavic Rus · defender</MfChip>
              <div className="mf-body" style={{ fontSize: 12, color: 'var(--gold-dim)', marginTop: 4 }}>
                defending <span style={{ color: 'var(--cream)', fontStyle: 'italic' }}>Kashgar</span>
              </div>
            </div>
            <MfPlaceholder style={{ width: 60, height: 60, border: '2px solid var(--slavic)' }}>DEF</MfPlaceholder>
          </div>
        </div>
        <MfOrnament style={{ margin: '12px 0' }} />
        <MfLabel>up to 2 stone dice · defender wins ties</MfLabel>
        <div style={{ display: 'flex', gap: 10, marginTop: 8, justifyContent: 'flex-end' }}>
          <MfDie value={5} type="def" />
          <MfDie value={3} type="def" />
        </div>
      </div>
    </div>

    {/* Result banner */}
    <div style={{ position: 'absolute', left: 50, right: 50, bottom: 30, zIndex: 2 }}>
      <div className="mf-panel tex-leather" style={{ padding: 16,
        display: 'grid', gridTemplateColumns: '1fr auto', gap: 18, alignItems: 'center',
        boxShadow: '0 6px 24px rgba(0,0,0,0.5)' }}>
        <div>
          <MfLabel>result</MfLabel>
          <div className="mf-title" style={{ fontSize: 17, marginTop: 4 }}>
            6 vs 5 → attacker wins · 4 vs 3 → attacker wins
          </div>
          <div className="mf-body" style={{ fontSize: 14, color: 'var(--gold-dim)', marginTop: 4 }}>
            Defender loses 2 warriors. Defender wins ties.
          </div>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          <MfBtn felt="timurid" glow>ATTACK AGAIN</MfBtn>
          <MfBtn>STOP</MfBtn>
        </div>
      </div>
    </div>
  </div>
);

// ===== Tooltip A: Compact felt =====
const MfTooltipA = () => (
  <div className="tex-wood" style={{ width: 600, height: 280, position: 'relative', padding: 30 }}>
    <div className="mf-panel tex-felt felt-kazakh" style={{ padding: 12, width: 270,
      boxShadow: '0 6px 20px rgba(0,0,0,0.5)' }}>
      <div className="mf-title" style={{ fontSize: 18 }}>Astana</div>
      <MfOrnament style={{ margin: '6px 0' }} />
      <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
        <MfChip faction="kazakh">Zhanibek Khan</MfChip>
        <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
          <span style={{ fontSize: 14, color: 'var(--gold-dim)' }}>⚔</span>
          <span className="mf-title" style={{ fontSize: 20 }}>5</span>
        </span>
      </div>
    </div>
    <div className="mf-body" style={{ position: 'absolute', right: 40, top: 60, fontSize: 13, color: 'var(--gold-dim)', fontStyle: 'italic' }}>
      ← hover any hex · felt tints to owner color
    </div>
  </div>
);

// ===== Game Over B: 4-up scoreboard =====
const MfGameOverB = () => (
  <div className="tex-wood" style={{ width: 1280, height: 760, position: 'relative', overflow: 'hidden' }}>
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(0,0,0,0.5)' }} />

    <div style={{ position: 'absolute', left: 50, right: 50, top: 40, bottom: 40, zIndex: 1 }}>
      <div className="mf-panel tex-leather" style={{ padding: 24, height: '100%',
        boxShadow: '0 10px 40px rgba(0,0,0,0.6)' }}>
        <MfOrnament variant="weave" />
        <div className="mf-title" style={{ fontFamily: 'var(--font-title)', fontSize: 32, textAlign: 'center', marginTop: 12,
          textShadow: '0 2px 10px rgba(0,0,0,0.5)' }}>The Khural Decides</div>
        <div className="mf-body" style={{ textAlign: 'center', color: 'var(--gold-dim)', fontStyle: 'italic' }}>
          The game is over · final standings
        </div>
        <MfOrnament variant="weave" style={{ marginTop: 10 }} />

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 14, marginTop: 22 }}>
          {[
            { f: 'kazakh', n: 'Zhanibek Khan', emp: 'Kazakh', t: 22, w: 35, win: true },
            { f: 'timurid', n: 'Tamerlane', emp: 'Timurid', t: 18, w: 28 },
            { f: 'slavic', n: 'Dmitry Donskoy', emp: 'Slavic Rus', t: 14, w: 20 },
            { f: 'ming', n: 'Emperor Yongle', emp: 'Ming', t: 10, w: 12 },
          ].map((e, i) => (
            <div key={i} className={`mf-panel tex-felt felt-${e.f}`} style={{
              padding: 14,
              boxShadow: e.win
                ? `0 0 0 2px var(--gold), 0 0 24px rgba(200,168,78,0.5)`
                : '0 4px 12px rgba(0,0,0,0.4)',
              borderColor: e.win ? 'var(--gold)' : `rgba(200,168,78,0.2)`,
            }}>
              {e.win && (
                <div style={{
                  padding: '2px 10px', background: 'rgba(200,168,78,0.2)', border: '1px solid var(--gold)',
                  borderRadius: 3, textAlign: 'center', marginBottom: 8,
                }}>
                  <span className="mf-title" style={{ fontSize: 11, color: 'var(--gold-bright)' }}>★ VICTOR ★</span>
                </div>
              )}
              <MfPlaceholder faction={e.f} style={{ height: 130, border: `2px solid var(--${e.f})` }}>
                {e.emp} portrait
              </MfPlaceholder>
              <div className="mf-title" style={{ fontSize: 15, marginTop: 8 }}>{e.n}</div>
              <MfLabel>{e.emp}</MfLabel>
              <MfOrnament variant="line" style={{ margin: '8px 0' }} />
              <div style={{ display: 'grid', gridTemplateColumns: '1fr auto', gap: 4, alignItems: 'center' }}>
                <span className="mf-body" style={{ fontSize: 12 }}>Territories</span>
                <span className="mf-title" style={{ fontSize: 14 }}>{e.t}</span>
                <span className="mf-body" style={{ fontSize: 12 }}>Warriors</span>
                <span className="mf-title" style={{ fontSize: 14 }}>{e.w}</span>
                <span className="mf-body" style={{ fontSize: 12 }}>Mission</span>
                <span className="mf-title" style={{ fontSize: 14, color: e.win ? 'var(--gold-bright)' : 'var(--gold-dim)' }}>
                  {e.win ? '✓' : '✗'}
                </span>
              </div>
            </div>
          ))}
        </div>

        <div style={{ display: 'flex', gap: 12, marginTop: 28, justifyContent: 'center' }}>
          <MfBtn felt="kazakh" glow>REMATCH</MfBtn>
          <MfBtn felt="slavic">MAIN MENU</MfBtn>
        </div>
      </div>
    </div>
  </div>
);

// ===== Hotseat A: Side-by-side picker =====
const MfHotseatA = () => (
  <div className="tex-wood" style={{ width: 1280, height: 760, position: 'relative', overflow: 'hidden' }}>
    <MfPlaceholder style={{ position: 'absolute', inset: 0, border: 'none', opacity: 0.15, borderRadius: 0 }}>
      Steppe background
    </MfPlaceholder>
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(0,0,0,0.3)' }} />

    {/* Title */}
    <div style={{ position: 'absolute', left: '50%', top: 24, transform: 'translateX(-50%)', textAlign: 'center', zIndex: 1 }}>
      <MfOrnament variant="weave" style={{ marginBottom: 8 }} />
      <div className="mf-title" style={{ fontFamily: 'var(--font-title)', fontSize: 28, textShadow: '0 2px 8px rgba(0,0,0,0.5)' }}>
        Choose Your Khans
      </div>
      <div className="mf-body" style={{ color: 'var(--gold-dim)', fontStyle: 'italic', marginTop: 4 }}>
        Select a leader for each player · hotseat mode
      </div>
    </div>

    {/* Two picker columns + VS */}
    <div style={{ position: 'absolute', left: 40, right: 40, top: 110, bottom: 80,
      display: 'grid', gridTemplateColumns: '1fr 70px 1fr', gap: 14, zIndex: 1 }}>

      {/* Player 1 */}
      <div className="mf-panel tex-leather" style={{ padding: 14, boxShadow: '0 6px 24px rgba(0,0,0,0.5)' }}>
        <MfLabel>player 1</MfLabel>
        <MfOrnament variant="line" style={{ margin: '6px 0' }} />
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 10 }}>
          {[
            { f: 'kazakh', n: 'Zhanibek Khan', emp: 'Kazakh', sel: true },
            { f: 'timurid', n: 'Tamerlane', emp: 'Timurid' },
            { f: 'slavic', n: 'Dmitry Donskoy', emp: 'Slavic Rus' },
            { f: 'ming', n: 'Emperor Yongle', emp: 'Ming' },
          ].map((l, i) => (
            <div key={i} className={`mf-panel tex-felt felt-${l.f}`} style={{
              padding: 10, cursor: 'pointer', transition: 'all 0.3s',
              boxShadow: l.sel ? `0 0 0 2px var(--gold), 0 0 16px rgba(200,168,78,0.4)` : '0 2px 8px rgba(0,0,0,0.3)',
              borderColor: l.sel ? 'var(--gold)' : 'rgba(200,168,78,0.15)',
            }}>
              <MfPlaceholder faction={l.f} style={{ height: 100, border: `2px solid var(--${l.f})` }}>{l.emp}</MfPlaceholder>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 6 }}>
                <div>
                  <div className="mf-title" style={{ fontSize: 12 }}>{l.n}</div>
                  <MfLabel>{l.emp}</MfLabel>
                </div>
                {l.sel && (
                  <span style={{
                    padding: '2px 6px', background: 'rgba(200,168,78,0.2)', border: '1px solid var(--gold)',
                    borderRadius: 3, fontFamily: 'var(--font-label)', fontSize: 9, color: 'var(--gold)',
                    textTransform: 'uppercase',
                  }}>selected</span>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* VS */}
      <div style={{ display: 'grid', placeItems: 'center' }}>
        <div style={{ textAlign: 'center' }}>
          <div className="mf-title" style={{ fontFamily: 'var(--font-title)', fontSize: 32 }}>VS</div>
          <MfOrnament variant="line" style={{ margin: '10px 0', width: 50 }} />
        </div>
      </div>

      {/* Player 2 */}
      <div className="mf-panel tex-leather" style={{ padding: 14, boxShadow: '0 6px 24px rgba(0,0,0,0.5)' }}>
        <MfLabel>player 2</MfLabel>
        <MfOrnament variant="line" style={{ margin: '6px 0' }} />
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 10 }}>
          {[
            { f: 'kazakh', n: 'Zhanibek Khan', emp: 'Kazakh', dis: true },
            { f: 'timurid', n: 'Tamerlane', emp: 'Timurid', sel: true },
            { f: 'slavic', n: 'Dmitry Donskoy', emp: 'Slavic Rus' },
            { f: 'ming', n: 'Emperor Yongle', emp: 'Ming' },
          ].map((l, i) => (
            <div key={i} className={`mf-panel tex-felt felt-${l.f}`} style={{
              padding: 10, cursor: l.dis ? 'not-allowed' : 'pointer', transition: 'all 0.3s',
              opacity: l.dis ? 0.35 : 1,
              boxShadow: l.sel ? `0 0 0 2px var(--gold), 0 0 16px rgba(200,168,78,0.4)` : '0 2px 8px rgba(0,0,0,0.3)',
              borderColor: l.sel ? 'var(--gold)' : 'rgba(200,168,78,0.15)',
            }}>
              <MfPlaceholder faction={l.f} style={{ height: 100, border: `2px solid var(--${l.f})` }}>{l.emp}</MfPlaceholder>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 6 }}>
                <div>
                  <div className="mf-title" style={{ fontSize: 12 }}>{l.n}</div>
                  <MfLabel>{l.emp}</MfLabel>
                </div>
                {l.sel && (
                  <span style={{
                    padding: '2px 6px', background: 'rgba(200,168,78,0.2)', border: '1px solid var(--gold)',
                    borderRadius: 3, fontFamily: 'var(--font-label)', fontSize: 9, color: 'var(--gold)',
                    textTransform: 'uppercase',
                  }}>selected</span>
                )}
                {l.dis && (
                  <span style={{
                    padding: '2px 6px', background: 'rgba(0,0,0,0.3)', border: '1px solid rgba(200,168,78,0.2)',
                    borderRadius: 3, fontFamily: 'var(--font-label)', fontSize: 9, color: 'var(--gold-dim)',
                    textTransform: 'uppercase',
                  }}>taken</span>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>

    {/* Bottom actions */}
    <div style={{ position: 'absolute', left: '50%', bottom: 20, transform: 'translateX(-50%)', display: 'flex', gap: 12, zIndex: 1 }}>
      <MfBtn>← BACK</MfBtn>
      <MfBtn felt="kazakh" glow>BEGIN CAMPAIGN →</MfBtn>
    </div>
  </div>
);

Object.assign(window, { MfCombatA, MfTooltipA, MfGameOverB, MfHotseatA });
