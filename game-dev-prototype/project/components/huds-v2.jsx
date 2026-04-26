/* ============ IN-GAME HUD v2 — Classic Risk, 4 phases, secret mission ============ */

// Empire roster row helper
const EmpireSlot = ({ f, name, territories, warriors, active }) => (
  <div className="box" style={{ padding: 6, background: `var(--${f}-soft)`,
    boxShadow: active ? `inset 0 0 0 2px var(--${f}), 0 0 12px rgba(185,137,42,0.35)` : 'none',
    transform: active ? 'scale(1.03)' : 'none' }}>
    <div style={{ display: 'grid', gridTemplateColumns: '30px 1fr auto', gap: 6, alignItems: 'center' }}>
      <Placeholder faction={f} style={{ width: 30, height: 30, fontSize: 7 }}>·</Placeholder>
      <div className="title-serif" style={{ fontSize: 13 }}>{name}</div>
      <div style={{ display: 'flex', gap: 8 }}>
        <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
          <span style={{ width: 12, height: 14, clipPath: 'polygon(50% 0, 100% 25%, 100% 75%, 50% 100%, 0 75%, 0 25%)',
            background: `var(--${f})`, display: 'inline-block' }}></span>
          <span className="title-serif" style={{ fontSize: 13 }}>{territories}</span>
        </span>
        <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
          <span style={{ fontSize: 11 }}>⚔</span>
          <span className="title-serif" style={{ fontSize: 13 }}>{warriors}</span>
        </span>
      </div>
    </div>
  </div>
);

// Phase pill helper
const PhasePill = ({ label, icon, active, style }) => (
  <div className="box" style={{
    padding: '4px 10px', display: 'flex', alignItems: 'center', gap: 4,
    background: active ? 'var(--ming-soft)' : 'var(--paper)',
    boxShadow: active ? '0 0 0 2px rgba(185,137,42,0.6), 0 0 14px rgba(185,137,42,0.35)' : 'none',
    ...style,
  }}>
    {icon && <span style={{ fontSize: 12 }}>{icon}</span>}
    <span className="title-serif" style={{ fontSize: 12 }}>{label}</span>
  </div>
);

// Secret mission element
const SecretMission = ({ text, style }) => (
  <div className="box" style={{
    padding: '8px 12px', background: 'var(--paper)',
    backgroundImage: 'repeating-linear-gradient(180deg, rgba(0,0,0,0.03) 0 4px, transparent 4px 10px)',
    borderStyle: 'dashed', position: 'relative', ...style,
  }}>
    <Tag>secret mission · wax-sealed parchment</Tag>
    <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 4 }}>
      <Glyph ch="🝊" size={16} />
      <div>
        <div className="note ink" style={{ fontSize: 13, lineHeight: 1.3 }}>{text}</div>
      </div>
    </div>
  </div>
);

// Variation A: Balanced — top bar + right empire roster
const HudA2 = () => (
  <div className="wf tint-kazakh" style={{ width: 1280, height: 760, position: 'relative' }}>
    {/* Map area */}
    <div style={{ position: 'absolute', inset: '68px 220px 0 0', background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper-2)' }}>
      <div style={{ position: 'absolute', left: 60, top: 50, display: 'grid', gridTemplateColumns: 'repeat(7, 56px)', gap: 4 }}>
        {Array.from({ length: 28 }).map((_, i) => {
          const f = ['kazakh','slavic','timurid','ming',null,'kazakh','slavic'][i%7];
          return <HexMini key={i} faction={f} label={f ? ['◇','✚','✶','☯','','◇','✚'][i%7] : ''} />;
        })}
      </div>
      <Tag style={{ position: 'absolute', left: 14, top: 14 }}>map viewport</Tag>
    </div>

    {/* Top bar — carved wood */}
    <div style={{ position: 'absolute', top: 0, left: 0, right: 0, height: 64,
      background: 'var(--paper-2)', borderBottom: '1.5px solid var(--ink)' }}>
      <Ornament style={{ position: 'absolute', left: 0, right: 0, bottom: -10 }} />
      <div style={{ display: 'grid', gridTemplateColumns: '200px 300px 1fr 80px', gap: 10, padding: '6px 12px', alignItems: 'center', height: '100%' }}>

        {/* Active leader */}
        <div className="box" style={{ padding: 6, background: 'var(--kazakh-soft)', display: 'grid', gridTemplateColumns: '44px 1fr', gap: 8, alignItems: 'center' }}>
          <Placeholder faction="kazakh" style={{ height: 44, fontSize: 8 }}>p1</Placeholder>
          <div>
            <div className="title-serif" style={{ fontSize: 15 }}>Zhanibek Khan</div>
            <Tag>Kazakh · player 1</Tag>
          </div>
        </div>

        {/* Phase tracker — 4 phases */}
        <div style={{ display: 'flex', gap: 4, alignItems: 'center' }}>
          <PhasePill label="SETUP" icon="⚑" />
          <PhasePill label="REINFORCE" icon="+" active />
          <PhasePill label="ATTACK" icon="⚔" />
          <PhasePill label="FORTIFY" icon="↔" />
          <span className="arrow" style={{ marginLeft: 2 }}>→</span>
        </div>

        {/* Secret mission */}
        <SecretMission text="Eliminate the Red Player" />

        {/* End phase */}
        <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
          <div style={{ width: 56, height: 56, borderRadius: '50%', border: '2.5px solid var(--ink)',
            background: 'var(--ming-soft)', display: 'grid', placeItems: 'center',
            boxShadow: '0 0 0 2px rgba(185,137,42,0.5), 0 0 18px rgba(185,137,42,0.4)' }}>
            <div style={{ fontFamily: 'Cormorant Garamond, serif', fontWeight: 700, fontSize: 10, textAlign: 'center', lineHeight: 1.1 }}>END<br/>PHASE</div>
          </div>
        </div>
      </div>
    </div>

    {/* Right empire roster */}
    <div style={{ position: 'absolute', right: 0, top: 68, bottom: 0, width: 210,
      borderLeft: '1.5px solid var(--ink)', background: 'var(--paper-2)', padding: 10 }}>
      <Tag>empire roster · permanent</Tag>
      <div className="title-serif" style={{ fontSize: 15, marginTop: 4 }}>Balance of Power</div>
      <Ornament variant="thin" style={{ margin: '6px 0' }} />
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        <EmpireSlot f="kazakh" name="Zhanibek Khan" territories={12} warriors={25} active />
        <EmpireSlot f="timurid" name="Tamerlane" territories={10} warriors={21} />
        <EmpireSlot f="slavic" name="Dmitry Donskoy" territories={11} warriors={18} />
        <EmpireSlot f="ming" name="Emperor Yongle" territories={9} warriors={16} />
      </div>
      <Ornament variant="thin" style={{ margin: '10px 0' }} />
      <Tag>active player highlighted + scale 1.03</Tag>
    </div>

    {/* Bottom-left tooltip */}
    <div style={{ position: 'absolute', left: 14, bottom: 14, width: 260 }}>
      <div className="box thick" style={{ padding: 10, background: 'var(--kazakh-soft)' }}>
        <div className="title-serif" style={{ fontSize: 16 }}>Astana</div>
        <Ornament variant="thin" style={{ margin: '6px 0' }} />
        <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
          <Chip faction="kazakh">Zhanibek Khan</Chip>
          <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
            <span style={{ fontSize: 12 }}>⚔</span>
            <span className="title-serif" style={{ fontSize: 16 }}>5</span>
          </span>
        </div>
      </div>
    </div>

    <Annotate x={340} y={46} w={100}>active phase glows gold</Annotate>
  </div>
);

// Variation B: Sparse — floating pill, info on demand
const HudB2 = () => (
  <div className="wf tint-timurid" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper-2)' }} />
    <div style={{ position: 'absolute', left: 200, top: 160, display: 'grid', gridTemplateColumns: 'repeat(8, 56px)', gap: 4 }}>
      {Array.from({ length: 24 }).map((_, i) => {
        const f = ['timurid',null,'kazakh',null,'slavic',null,'ming',null][i%8];
        return <HexMini key={i} faction={f} label={f ? '·' : ''} />;
      })}
    </div>

    {/* Floating compact bar — top center */}
    <div style={{ position: 'absolute', left: '50%', top: 16, transform: 'translateX(-50%)' }}>
      <div className="box thick" style={{ padding: '6px 14px', background: 'var(--paper)',
        display: 'flex', gap: 12, alignItems: 'center' }}>
        <Placeholder faction="timurid" style={{ width: 32, height: 32, fontSize: 7 }}>·</Placeholder>
        <span className="title-serif" style={{ fontSize: 16 }}>Tamerlane</span>
        <div style={{ width: 1, height: 24, background: 'var(--ink)' }} />
        <PhasePill label="ATTACK" icon="⚔" active style={{ margin: 0 }} />
        <div style={{ width: 1, height: 24, background: 'var(--ink)' }} />
        <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
          <span style={{ width: 10, height: 12, clipPath: 'polygon(50% 0, 100% 25%, 100% 75%, 50% 100%, 0 75%, 0 25%)',
            background: 'var(--timurid)', display: 'inline-block' }}></span>
          <span className="title-serif" style={{ fontSize: 14 }}>10</span>
        </span>
        <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
          <span style={{ fontSize: 11 }}>⚔</span>
          <span className="title-serif" style={{ fontSize: 14 }}>21</span>
        </span>
      </div>
    </div>

    {/* Secret mission — top left, unrolled parchment */}
    <div style={{ position: 'absolute', left: 14, top: 14, width: 220 }}>
      <SecretMission text="Control 24 territories" />
    </div>

    {/* End phase — bottom right */}
    <div style={{ position: 'absolute', right: 30, bottom: 30, textAlign: 'center' }}>
      <div style={{ width: 80, height: 80, borderRadius: '50%', border: '2.5px solid var(--ink)',
        background: 'var(--ming-soft)', display: 'grid', placeItems: 'center',
        boxShadow: '0 0 0 2px rgba(185,137,42,0.6), 0 0 22px rgba(185,137,42,0.4)' }}>
        <div className="title-serif" style={{ fontSize: 12, textAlign: 'center', lineHeight: 1.1 }}>END<br/>PHASE</div>
      </div>
    </div>

    {/* Slim left rail — phases */}
    <div style={{ position: 'absolute', left: 14, top: 260, display: 'flex', flexDirection: 'column', gap: 8 }}>
      {[
        { l: 'S', icon: '⚑', label: 'SETUP' },
        { l: 'R', icon: '+', label: 'REINFORCE' },
        { l: 'A', icon: '⚔', label: 'ATTACK', active: true },
        { l: 'F', icon: '↔', label: 'FORTIFY' },
      ].map((p, i) => (
        <div key={i} style={{
          width: 40, height: 40, border: '1.5px solid var(--ink)', borderRadius: 4,
          background: p.active ? 'var(--ming-soft)' : 'var(--paper)',
          display: 'grid', placeItems: 'center',
          boxShadow: p.active ? '0 0 0 2px rgba(185,137,42,0.5)' : 'none',
        }}>
          <span className="title-serif" style={{ fontSize: 16 }}>{p.l}</span>
        </div>
      ))}
    </div>

    {/* Hover tooltip */}
    <div style={{ position: 'absolute', left: 360, top: 420, width: 220 }}>
      <Tag>on hover</Tag>
      <div className="box" style={{ padding: 8, marginTop: 4, background: 'var(--slavic-soft)' }}>
        <div className="title-serif" style={{ fontSize: 14 }}>Kashgar</div>
        <div style={{ display: 'flex', gap: 6, marginTop: 4, alignItems: 'center' }}>
          <Chip faction="slavic">Donskoy</Chip>
          <span>⚔</span><span className="title-serif" style={{ fontSize: 14 }}>3</span>
        </div>
      </div>
    </div>

    {/* Empire roster — collapsed dots bottom-left */}
    <div style={{ position: 'absolute', left: 14, bottom: 14, display: 'flex', gap: 6 }}>
      {[
        { f: 'kazakh', t: 12, w: 25 },
        { f: 'timurid', t: 10, w: 21, active: true },
        { f: 'slavic', t: 11, w: 18 },
        { f: 'ming', t: 9, w: 16 },
      ].map((e, i) => (
        <div key={i} className="box" style={{ padding: '4px 8px', background: `var(--${e.f}-soft)`,
          boxShadow: e.active ? `inset 0 0 0 2px var(--${e.f})` : 'none',
          display: 'flex', gap: 6, alignItems: 'center' }}>
          <span style={{ width: 8, height: 8, borderRadius: '50%', background: `var(--${e.f})`, display: 'inline-block' }}></span>
          <Tag>{e.t}·{e.w}</Tag>
        </div>
      ))}
      <Tag style={{ alignSelf: 'center' }}>expand →</Tag>
    </div>

    <Annotate x={600} y={24} w={300}>minimal: everything else hides until needed</Annotate>
  </div>
);

// Variation C: Rich — full chrome, bottom dock, Civ-like density
const HudC2 = () => (
  <div className="wf tint-slavic" style={{ width: 1280, height: 760, position: 'relative' }}>
    {/* Top bar */}
    <div style={{ position: 'absolute', top: 0, left: 0, right: 0, height: 60, background: 'var(--paper-2)',
      borderBottom: '1.5px solid var(--ink)', display: 'grid',
      gridTemplateColumns: '210px 280px 1fr 240px 70px', gap: 8, padding: '4px 10px', alignItems: 'center' }}>

      {/* Active leader */}
      <div className="box" style={{ padding: 5, background: 'var(--slavic-soft)', display: 'grid', gridTemplateColumns: '40px 1fr', gap: 6, alignItems: 'center' }}>
        <Placeholder faction="slavic" style={{ width: 40, height: 40, fontSize: 8 }}>p3</Placeholder>
        <div>
          <div className="title-serif" style={{ fontSize: 14 }}>Dmitry Donskoy</div>
          <Tag>Slavic Rus · P3</Tag>
        </div>
      </div>

      {/* Phase tracker */}
      <div style={{ display: 'flex', gap: 3, alignItems: 'center' }}>
        <PhasePill label="SETUP" icon="⚑" />
        <PhasePill label="REINFORCE" icon="+" />
        <PhasePill label="ATTACK" icon="⚔" active />
        <PhasePill label="FORTIFY" icon="↔" />
      </div>

      {/* Secret mission */}
      <SecretMission text="Eliminate the Red Player" />

      {/* Reinforcement info (shown during REINFORCE) */}
      <div className="box" style={{ padding: '4px 8px', background: 'var(--paper)' }}>
        <Tag>reinforce (when active)</Tag>
        <div style={{ display: 'flex', gap: 6, alignItems: 'center', marginTop: 2 }}>
          <span className="title-serif" style={{ fontSize: 14 }}>+max(3, 11/3) = </span>
          <span className="title-serif" style={{ fontSize: 18 }}>+4</span>
          <Tag>warriors</Tag>
        </div>
      </div>

      {/* End phase */}
      <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
        <div style={{ width: 50, height: 50, borderRadius: '50%', border: '2.5px solid var(--ink)',
          background: 'var(--ming-soft)', display: 'grid', placeItems: 'center',
          boxShadow: '0 0 0 2px rgba(185,137,42,0.5), 0 0 18px rgba(185,137,42,0.4)' }}>
          <div style={{ fontFamily: 'Cormorant Garamond, serif', fontWeight: 700, fontSize: 9, textAlign: 'center', lineHeight: 1.1 }}>END<br/>PHASE</div>
        </div>
      </div>
    </div>

    {/* Map */}
    <div style={{ position: 'absolute', inset: '64px 240px 100px 0', background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper-2)' }}>
      <div style={{ position: 'absolute', left: 60, top: 40, display: 'grid', gridTemplateColumns: 'repeat(7, 56px)', gap: 4 }}>
        {Array.from({ length: 28 }).map((_, i) => {
          const f = ['slavic','slavic','kazakh',null,'ming',null,'timurid'][i%7];
          return <HexMini key={i} faction={f} label={f ? '·' : ''} />;
        })}
      </div>
    </div>

    {/* Right empire roster — always visible */}
    <div style={{ position: 'absolute', right: 0, top: 64, bottom: 100, width: 232,
      background: 'var(--paper-2)', borderLeft: '1.5px solid var(--ink)', padding: 10 }}>
      <Ornament variant="thin" />
      <div className="title-serif" style={{ fontSize: 15, margin: '6px 0' }}>Empire Roster</div>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        <EmpireSlot f="kazakh" name="Zhanibek Khan" territories={12} warriors={25} />
        <EmpireSlot f="timurid" name="Tamerlane" territories={10} warriors={21} />
        <EmpireSlot f="slavic" name="Dmitry Donskoy" territories={11} warriors={18} active />
        <EmpireSlot f="ming" name="Emperor Yongle" territories={9} warriors={16} />
      </div>
      <Ornament variant="thin" style={{ margin: '10px 0' }} />
      <Tag>⚑ = setup · + = reinforce · ⚔ = warriors · ↔ = fortify</Tag>
    </div>

    {/* Bottom dock — tooltip + action log */}
    <div style={{ position: 'absolute', left: 0, right: 0, bottom: 0, height: 96, background: 'var(--paper-2)',
      borderTop: '1.5px solid var(--ink)', display: 'grid', gridTemplateColumns: '280px 1fr 200px', gap: 10, padding: 8 }}>

      {/* Tooltip */}
      <div className="box" style={{ padding: 8, background: 'var(--slavic-soft)' }}>
        <div className="title-serif" style={{ fontSize: 16 }}>Vladimir</div>
        <div style={{ display: 'flex', gap: 8, marginTop: 4, alignItems: 'center' }}>
          <Chip faction="slavic">Donskoy</Chip>
          <span>⚔</span><span className="title-serif" style={{ fontSize: 16 }}>4</span>
        </div>
      </div>

      {/* Action log */}
      <div className="box" style={{ padding: 8, overflow: 'hidden' }}>
        <Tag>action log</Tag>
        <div style={{ fontSize: 11, lineHeight: 1.4, marginTop: 2, fontFamily: 'Special Elite, monospace' }}>
          <div>▸ Donskoy attacks Kashgar from Vladimir (3 vs 2)</div>
          <div>▸ Donskoy reinforced +4 warriors at Moscow</div>
          <div style={{ opacity: 0.5 }}>▸ Tamerlane fortified 2 warriors Samarqand → Bukhara</div>
        </div>
      </div>

      {/* Minimap */}
      <div className="box" style={{ padding: 4 }}>
        <Tag>minimap</Tag>
        <div style={{ height: '100%', background: 'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 6px)',
          border: '1px solid var(--ink)', position: 'relative' }}>
          <div style={{ position: 'absolute', left: 6, top: 6, width: 20, height: 14, background: 'var(--slavic)', opacity: 0.5 }} />
          <div style={{ position: 'absolute', left: 40, top: 25, width: 18, height: 14, background: 'var(--kazakh)', opacity: 0.5 }} />
          <div style={{ position: 'absolute', right: 10, top: 10, width: 16, height: 12, background: 'var(--timurid)', opacity: 0.5 }} />
          <div style={{ position: 'absolute', right: 4, bottom: 4, width: 20, height: 14, background: 'var(--ming)', opacity: 0.5 }} />
        </div>
      </div>
    </div>
  </div>
);

Object.assign(window, { HudA2, HudB2, HudC2, EmpireSlot, PhasePill, SecretMission });
