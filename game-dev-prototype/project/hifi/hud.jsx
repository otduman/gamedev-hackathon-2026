/* ====== Hi-fi: HUD v2 — Redesigned: breathable top bar, mission in right panel, bigger map ====== */

const HfHud = ({ onCombat, onEndTurn, gameState, setGameState }) => {
  const { activePlayer, phase, showTooltip, tooltipData } = gameState;
  const leader = LEADERS[activePlayer];
  const [showAnnotations, setShowAnnotations] = React.useState(false);

  React.useEffect(() => {
    const handler = (e) => { if (e.key === 'a' || e.key === 'A') setShowAnnotations(v => !v); };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, []);

  const advancePhase = () => {
    const idx = PHASES.findIndex(p => p.id === phase);
    if (idx < PHASES.length - 1) {
      setGameState(s => ({ ...s, phase: PHASES[idx + 1].id }));
    } else {
      const factions = ['kazakh', 'timurid', 'slavic', 'ming'];
      const next = factions[(factions.indexOf(activePlayer) + 1) % 4];
      setGameState(s => ({ ...s, activePlayer: next, phase: 'reinforce' }));
    }
  };

  const empireData = [
    { f: 'kazakh', t: 12, w: 25 },
    { f: 'timurid', t: 10, w: 21 },
    { f: 'slavic', t: 11, w: 18 },
    { f: 'ming', t: 9, w: 16 },
  ];

  const missions = {
    kazakh: 'Eliminate the Red Player',
    timurid: 'Control 24 Territories',
    slavic: 'Conquer Asia and Africa',
    ming: 'Eliminate the Blue Player',
  };

  // Bigger hex grid data — 10 columns × 8 rows = 80 hexes
  const hexData = [];
  const fPool = ['kazakh','slavic','timurid','ming', null];
  const cityNames = ['Astana','Almaty','Moscow','Samarqand','Beijing','Turkestan','Kiev','Kyzylorda',
    'Bukhara','Nanjing','Novgorod','Atyrau','Herat','Aktobe','Vladimir','Luoyang','Shymkent','Merv',
    'Pskov','Taraz','Xian','Tabriz','Ryazan','Semey','Guangzhou','Oral','Isfahan','Tver',
    'Kostanay','Suzhou','Pavlodar','Kashgar','Sarai','Karakorum','Urgench','Otrar','Balkh','Dunhuang',
    'Tashkent','Fergana','Kokand','Khiva','Derbent','Baku','Tbilisi','Yerevan','Mosul','Aleppo',
    'Damascus','Baghdad','Basra','Muscat','Kabul','Lahore','Delhi','Agra','Jaipur','Multan',
    'Karachi','Hormuz','Shiraz','Kerman','Nishapur','Mashhad','Ashgabat','Mary','Turpan','Urumqi',
    'Hotan','Yarkand','Kucha','Lhasa','Chengdu','Wuhan','Hangzhou','Fuzhou','Quanzhou','Kunming','Dali','Lijiang'];
  for (let i = 0; i < 80; i++) {
    // deterministic-ish faction based on position
    const col = i % 10;
    const row = Math.floor(i / 10);
    let f = null;
    if (col < 3 && row < 4) f = 'kazakh';
    else if (col >= 7 && row < 4) f = 'ming';
    else if (col < 4 && row >= 4) f = 'slavic';
    else if (col >= 6 && row >= 4) f = 'timurid';
    else if (Math.random() > 0.4) f = fPool[Math.floor(Math.random() * 4)];
    hexData.push({ f, name: cityNames[i] || '' });
  }

  return (
    <div className="t-wood-dark" style={{ width: 1920, height: 1080, position: 'relative', overflow: 'hidden' }}>
      {/* Map area — fills most of screen */}
      <div style={{ position: 'absolute', inset: '72px 290px 0 0',
        background: 'linear-gradient(135deg, #0e1a0a 0%, #1a2810 40%, #141e0c 100%)' }}>
        {/* Hex grid — 10 col × 14 rows, staggered, filling viewport */}
        <div style={{ position: 'absolute', left: 20, top: 12, right: 10, bottom: 10 }}>
          {Array.from({ length: 14 }).map((_, row) => (
            <div key={row} style={{
              display: 'flex', gap: 3, marginTop: row === 0 ? 0 : -18,
              marginLeft: row % 2 === 1 ? 39 : 0,
            }}>
              {Array.from({ length: 10 }).map((_, col) => {
                const idx = row * 10 + col;
                const hex = hexData[idx % hexData.length];
                const f = hex?.f;
                return (
                  <div key={col}
                    className="pointer transition"
                    onMouseEnter={() => f && setGameState(s => ({ ...s, showTooltip: true,
                      tooltipData: { name: hex.name || 'Unknown', owner: f, warriors: 1 + Math.floor(Math.random() * 6) }}))}
                    onMouseLeave={() => setGameState(s => ({ ...s, showTooltip: false }))}
                    onClick={() => { if (f && phase === 'attack' && f !== activePlayer) onCombat && onCombat(f); }}
                    style={{
                      width: 74, height: 84, flexShrink: 0,
                      clipPath: 'polygon(50% 0, 100% 25%, 100% 75%, 50% 100%, 0 75%, 0 25%)',
                      background: f
                        ? `radial-gradient(circle at 50% 40%, var(--${f}-felt) 0%, var(--${f}-bg) 100%)`
                        : 'rgba(255,255,255,0.03)',
                      display: 'grid', placeItems: 'center',
                      transition: 'all 0.15s', cursor: f ? 'pointer' : 'default',
                    }}>
                    {f && <span style={{ fontSize: 8, color: `var(--${f}-light)`, opacity: 0.9,
                      fontFamily: 'var(--font-label)', textAlign: 'center', lineHeight: 1.1,
                      textShadow: '0 1px 3px rgba(0,0,0,0.8)',
                    }}>
                      {hex.name?.substring(0, 6) || ''}
                    </span>}
                  </div>
                );
              })}
            </div>
          ))}
        </div>
        <Embers count={8} />
      </div>

      {/* === TOP BAR — cleaner: leader + phases + end phase only === */}
      <div className="t-wood anim-fade" style={{
        position: 'absolute', top: 0, left: 0, right: 0, height: 68,
        borderBottom: '2px solid var(--brass)',
        boxShadow: '0 4px 20px rgba(0,0,0,0.6), inset 0 -1px 0 rgba(255,255,255,0.03)',
        zIndex: 10,
      }}>
        <Orn variant="dots" style={{ position: 'absolute', left: 0, right: 0, bottom: -9 }} />
        <div style={{ display: 'flex', alignItems: 'center', height: '100%', padding: '0 16px', gap: 20 }}>

          {/* Active leader — generous width */}
          <Panel felt={activePlayer} style={{ padding: '6px 14px', flexShrink: 0,
            display: 'flex', gap: 12, alignItems: 'center' }}>
            <Portrait faction={activePlayer} size={48} glow />
            <div>
              <HfTitle size={16}>{leader.name}</HfTitle>
              <HfLabel>{leader.empire} · active</HfLabel>
            </div>
            {showAnnotations && <UiSpec text="Panel_ActiveLeader" style={{ top: -14, left: 0 }} />}
          </Panel>

          {/* Spacer + ornament */}
          <div style={{ width: 1, height: 36, background: 'var(--brass-border)' }} />

          {/* Phase tracker — centered, with room */}
          <div style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
            {PHASES.map(p => (
              <HfPhasePill key={p.id} phase={p} active={phase === p.id}
                onClick={() => setGameState(s => ({ ...s, phase: p.id }))} />
            ))}
            {showAnnotations && <UiSpec text="Panel_PhaseTracker" style={{ top: -14, left: 0 }} />}
          </div>

          <div style={{ width: 1, height: 36, background: 'var(--brass-border)' }} />

          {/* Reinforce info (contextual) */}
          <div style={{ flex: 1, fontFamily: 'var(--font-body)', fontSize: 14, color: 'var(--gold-dim)', fontStyle: 'italic' }}>
            {phase === 'reinforce' && `+${Math.max(3, Math.floor((empireData.find(e => e.f === activePlayer)?.t || 0) / 3))} warriors to place`}
            {phase === 'attack' && 'Click enemy territory to attack'}
            {phase === 'fortify' && 'Move warriors between your territories'}
            {phase === 'setup' && 'Place your initial 25 warriors'}
          </div>

          {/* End phase */}
          <HfEndPhase size={52} active={phase === 'attack' || phase === 'fortify'} onClick={advancePhase} />
        </div>

        {showAnnotations && <SfxCue text="phase_advance_brass_clank" style={{ bottom: -14, right: 100 }} />}
      </div>

      {/* === RIGHT PANEL — empire roster + secret mission === */}
      <div className="t-leather anim-fade anim-d3" style={{
        position: 'absolute', right: 0, top: 72, bottom: 0, width: 284,
        borderLeft: '2px solid var(--brass)', padding: 14,
        boxShadow: '-4px 0 20px rgba(0,0,0,0.5)', zIndex: 10,
        display: 'flex', flexDirection: 'column',
      }}>
        {/* Secret mission — moved here, prominent */}
        <HfMission text={missions[activePlayer]} style={{ marginBottom: 12 }} />

        <Orn style={{ marginBottom: 10 }} />

        <HfLabel>empire roster</HfLabel>
        <HfTitle size={14} style={{ display: 'block', marginTop: 4, marginBottom: 8 }}>Balance of Power</HfTitle>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 8, flex: 1 }}>
          {empireData.map(e => (
            <HfEmpireSlot key={e.f} faction={e.f} territories={e.t} warriors={e.w}
              active={e.f === activePlayer} />
          ))}
        </div>

        <Orn style={{ margin: '10px 0' }} />

        {/* Turn counter at bottom of panel */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <HfLabel>turn</HfLabel>
            <HfTitle size={20} style={{ display: 'block' }}>17</HfTitle>
          </div>
          <div style={{ textAlign: 'right' }}>
            <HfLabel>total territories</HfLabel>
            <HfTitle size={20} style={{ display: 'block' }}>42</HfTitle>
          </div>
        </div>

        {showAnnotations && <>
          <UiSpec text="Panel_RightSidebar [VLayout] → Mission+Roster+TurnInfo" style={{ top: -14, left: 10 }} />
        </>}
      </div>

      {/* === BOTTOM-LEFT TOOLTIP === */}
      {showTooltip && tooltipData && (
        <div className="anim-fade" style={{ position: 'absolute', left: 20, bottom: 20, width: 300, zIndex: 20 }}>
          <Panel felt={tooltipData.owner} style={{
            padding: 14, boxShadow: '0 6px 24px rgba(0,0,0,0.6)' }}>
            <HfTitle size={20}>{tooltipData.name}</HfTitle>
            <Orn style={{ margin: '8px 0' }} />
            <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
              <Portrait faction={tooltipData.owner} size={36} />
              <HfChip faction={tooltipData.owner}>{LEADERS[tooltipData.owner]?.name}</HfChip>
              <span style={{ display: 'flex', alignItems: 'center', gap: 4, marginLeft: 'auto' }}>
                <span style={{ fontSize: 15, color: 'var(--gold-dim)' }}>⚔</span>
                <HfTitle size={22}>{tooltipData.warriors}</HfTitle>
              </span>
            </div>
          </Panel>
          {showAnnotations && <UiSpec text="Panel_TileTooltip" style={{ top: -14, left: 0 }} />}
        </div>
      )}

      {/* Annotations hint */}
      <div style={{ position: 'absolute', right: 300, bottom: 16, zIndex: 10 }}>
        <HfLabel style={{ opacity: 0.4 }}>press A for Unity spec + SFX annotations</HfLabel>
      </div>
    </div>
  );
};

Object.assign(window, { HfHud });
