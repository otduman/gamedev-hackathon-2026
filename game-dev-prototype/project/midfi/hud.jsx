/* ====== Mid-fi: HUD A — Balanced top bar + side roster ====== */

const MfHudA = () => (
  <div className="tex-wood" style={{ width: 1280, height: 760, position: 'relative', overflow: 'hidden' }}>
    {/* Map area */}
    <div style={{ position: 'absolute', inset: '68px 220px 0 0',
      background: 'linear-gradient(135deg, #2a3a1a 0%, #3a4a2a 50%, #2a3a1a 100%)' }}>
      <div style={{ position: 'absolute', left: 50, top: 40, display: 'grid', gridTemplateColumns: 'repeat(8, 54px)', gap: 3 }}>
        {Array.from({ length: 40 }).map((_, i) => {
          const factions = ['kazakh','kazakh','slavic',null,'timurid','ming',null,'kazakh'];
          const f = factions[i % 8];
          return (
            <div key={i} className="mf-hex" style={{
              background: f ? `var(--${f}-bg)` : 'rgba(0,0,0,0.3)',
              border: f ? `1px solid var(--${f})` : '1px solid rgba(200,168,78,0.1)',
              clipPath: 'polygon(50% 0, 100% 25%, 100% 75%, 50% 100%, 0 75%, 0 25%)',
            }}>
              <span style={{ fontSize: 9, opacity: 0.7 }}>{f ? ['◇','◇','✚','','✶','☯','','◇'][i%8] : ''}</span>
            </div>
          );
        })}
      </div>
      <MfLabel style={{ position: 'absolute', left: 10, top: 10 }}>map viewport</MfLabel>
    </div>

    {/* Top bar */}
    <div className="tex-wood" style={{ position: 'absolute', top: 0, left: 0, right: 0, height: 64,
      borderBottom: '2px solid var(--brass)',
      boxShadow: '0 4px 16px rgba(0,0,0,0.5), inset 0 -1px 0 rgba(255,255,255,0.04)' }}>
      <MfOrnament variant="dots" style={{ position: 'absolute', left: 0, right: 0, bottom: -8 }} />
      <div style={{ display: 'grid', gridTemplateColumns: '210px 290px 1fr 70px', gap: 10, padding: '6px 12px', alignItems: 'center', height: '100%' }}>

        {/* Active leader */}
        <div className="mf-panel tex-felt felt-kazakh" style={{ padding: 6, display: 'grid', gridTemplateColumns: '44px 1fr', gap: 8, alignItems: 'center' }}>
          <MfPlaceholder style={{ width: 44, height: 44, fontSize: 8, border: '2px solid var(--kazakh)' }}>P1</MfPlaceholder>
          <div>
            <div className="mf-title" style={{ fontSize: 14 }}>Zhanibek Khan</div>
            <MfLabel>Kazakh · player 1</MfLabel>
          </div>
        </div>

        {/* Phase tracker */}
        <div style={{ display: 'flex', gap: 4, alignItems: 'center' }}>
          <MfPhasePill label="SETUP" icon="⚑" />
          <MfPhasePill label="REINFORCE" icon="+" active />
          <MfPhasePill label="ATTACK" icon="⚔" />
          <MfPhasePill label="FORTIFY" icon="↔" />
        </div>

        {/* Secret mission */}
        <MfSecretMission text="Eliminate the Red Player" />

        {/* End phase */}
        <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
          <MfEndPhase size={52} />
        </div>
      </div>
    </div>

    {/* Right empire roster */}
    <div className="tex-leather" style={{ position: 'absolute', right: 0, top: 68, bottom: 0, width: 214,
      borderLeft: '2px solid var(--brass)', padding: 10,
      boxShadow: '-4px 0 16px rgba(0,0,0,0.4)' }}>
      <MfLabel>empire roster</MfLabel>
      <div className="mf-title" style={{ fontSize: 14, marginTop: 4 }}>Balance of Power</div>
      <MfOrnament variant="line" style={{ margin: '8px 0' }} />
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        <MfEmpireSlot faction="kazakh" name="Zhanibek Khan" territories={12} warriors={25} active />
        <MfEmpireSlot faction="timurid" name="Tamerlane" territories={10} warriors={21} />
        <MfEmpireSlot faction="slavic" name="Dmitry Donskoy" territories={11} warriors={18} />
        <MfEmpireSlot faction="ming" name="Emperor Yongle" territories={9} warriors={16} />
      </div>
      <MfOrnament variant="line" style={{ margin: '10px 0' }} />
      <MfLabel>active player highlighted</MfLabel>
    </div>

    {/* Bottom-left tooltip */}
    <div style={{ position: 'absolute', left: 14, bottom: 14, width: 260, zIndex: 2 }}>
      <div className="mf-panel tex-felt felt-kazakh" style={{ padding: 10,
        boxShadow: '0 4px 16px rgba(0,0,0,0.5)' }}>
        <div className="mf-title" style={{ fontSize: 17 }}>Astana</div>
        <MfOrnament variant="line" style={{ margin: '6px 0' }} />
        <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
          <MfChip faction="kazakh">Zhanibek Khan</MfChip>
          <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
            <span style={{ fontSize: 13, color: 'var(--gold-dim)' }}>⚔</span>
            <span className="mf-title" style={{ fontSize: 18 }}>5</span>
          </span>
        </div>
      </div>
    </div>
  </div>
);

Object.assign(window, { MfHudA });
