/* ============ IN-GAME HUD — 3 variations ============ */

// Variation A: Top bar continuous + right collapsible empire panel + bottom-left tooltip
const HudA = () => (
  <div className="wf tint-kazakh" style={{ width: 1280, height: 760, position: 'relative' }}>
    {/* Map area */}
    <div style={{ position: 'absolute', inset: '70px 220px 110px 0', background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper-2)' }}>
      <div style={{ position: 'absolute', left: 80, top: 60, display: 'grid', gridTemplateColumns: 'repeat(7, 56px)', gap: 4 }}>
        {Array.from({ length: 35 }).map((_, i) => {
          const f = ['kazakh','slavic','timurid','ming',null,null,'kazakh'][i%7];
          return <HexMini key={i} faction={f} label={['◇','✚','✶','☯','','','◇'][i%7]} />;
        })}
      </div>
      <Tag style={{ position: 'absolute', left: 14, top: 14 }}>map viewport</Tag>
    </div>

    {/* Top bar */}
    <div style={{ position: 'absolute', top: 0, left: 0, right: 0, height: 64,
      background: 'var(--paper-2)', borderBottom: '1.5px solid var(--ink)' }}>
      <Ornament style={{ position: 'absolute', left: 0, right: 0, bottom: -10 }} />
      <div style={{ display: 'grid', gridTemplateColumns: '180px 320px 1fr 240px', gap: 14, padding: '8px 14px', alignItems: 'center', height: '100%' }}>
        {/* Turn + Phase */}
        <div className="box" style={{ padding: '6px 10px', background: 'var(--paper)' }}>
          <Tag>turn</Tag>
          <div className="title-serif" style={{ fontSize: 22 }}>17 / —</div>
          <div style={{ display: 'flex', gap: 4, marginTop: 2 }}>
            <Label style={{ background: 'var(--kazakh-soft)' }}>REINFORCE</Label>
            <Label>action</Label>
            <Label>migr.</Label>
          </div>
        </div>
        {/* Active leader */}
        <div className="box" style={{ padding: 6, background: 'var(--kazakh-soft)', display: 'grid', gridTemplateColumns: '46px 1fr', gap: 8, alignItems: 'center' }}>
          <Placeholder faction="kazakh" style={{ height: 46, fontSize: 8 }}>p1</Placeholder>
          <div>
            <div className="title-serif" style={{ fontSize: 16 }}>Zhanibek-khan</div>
            <Chip faction="kazakh">Kazakh · active</Chip>
          </div>
        </div>
        {/* Prestige */}
        <div className="box" style={{ padding: 6, background: 'var(--paper)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{ width: 38, height: 38, borderRadius: '50%', border: '1.5px solid var(--ink)',
              background: 'conic-gradient(var(--kazakh) 0 168deg, var(--paper-2) 168deg 360deg)',
              display: 'grid', placeItems: 'center' }}>
              <div style={{ width: 26, height: 26, borderRadius: '50%', background: 'var(--paper)',
                display: 'grid', placeItems: 'center', fontFamily: 'Cormorant Garamond, serif', fontWeight: 700 }}>7</div>
            </div>
            <div>
              <Tag>prestige</Tag>
              <div className="title-serif" style={{ fontSize: 18 }}>7 / 15</div>
            </div>
            <div style={{ marginLeft: 'auto' }}>
              <Tag>reinforce</Tag>
              <div className="note ink">+max(3, owned/3) = 5</div>
            </div>
          </div>
        </div>
        {/* End turn */}
        <div style={{ display: 'flex', gap: 8, alignItems: 'center', justifyContent: 'flex-end' }}>
          <div style={{ textAlign: 'center' }}>
            <Tag>AP</Tag><div className="title-serif" style={{ fontSize: 18 }}>2/3</div>
          </div>
          <div style={{ textAlign: 'center' }}>
            <Tag>MP</Tag><div className="title-serif" style={{ fontSize: 18 }}>2</div>
          </div>
          <div style={{ width: 64, height: 64, borderRadius: '50%', border: '2.5px solid var(--ink)',
            background: 'var(--ming-soft)', display: 'grid', placeItems: 'center', position: 'relative',
            boxShadow: '0 0 0 2px rgba(185,137,42,0.5), 0 0 20px rgba(185,137,42,0.45)' }}>
            <div style={{ fontFamily: 'Cormorant Garamond, serif', fontWeight: 700, fontSize: 11, textAlign: 'center', lineHeight: 1.1 }}>END<br/>TURN</div>
            <span className="strap l" style={{ left: 8, top: -4, bottom: -4 }}></span>
            <span className="strap r" style={{ right: 8, top: -4, bottom: -4 }}></span>
          </div>
        </div>
      </div>
    </div>

    {/* Right empire panel */}
    <div style={{ position: 'absolute', right: 0, top: 70, bottom: 110, width: 210,
      borderLeft: '1.5px solid var(--ink)', background: 'var(--paper-2)', padding: 10 }}>
      <Tag>empires · collapsible</Tag>
      <div className="title-serif" style={{ fontSize: 16, marginTop: 4 }}>Standings</div>
      <Ornament variant="thin" style={{ margin: '6px 0' }} />
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        {[
          { f: 'kazakh', n: 'Argyn', p: 7, t: 14, y: 3, active: true },
          { f: 'timurid', n: 'Naiman', p: 5, t: 11, y: 2 },
          { f: 'slavic', n: 'Donskoy', p: 4, t: 9, y: 2 },
          { f: 'ming', n: 'Yongle', p: 6, t: 12, y: 3 },
        ].map((e, i) => (
          <div key={i} className="box" style={{ padding: 6, background: `var(--${e.f}-soft)`,
            boxShadow: e.active ? `inset 0 0 0 2px var(--${e.f})` : 'none' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Chip faction={e.f}>{e.n}</Chip>
              <span className="title-serif" style={{ fontSize: 14 }}>{e.p}/15</span>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 4 }}>
              <Tag>tiles {e.t}</Tag><Tag>yurts {e.y}</Tag>
            </div>
            <div style={{ height: 6, background: 'var(--paper)', border: '1px solid var(--ink)', marginTop: 4, position: 'relative' }}>
              <div style={{ position: 'absolute', inset: 0, width: `${e.p/15*100}%`, background: `var(--${e.f})` }} />
            </div>
          </div>
        ))}
      </div>
    </div>

    {/* Bottom-left tooltip */}
    <div style={{ position: 'absolute', left: 14, bottom: 14, width: 320 }}>
      <Tag>tile tooltip · weathered felt</Tag>
      <div className="box thick" style={{ padding: 10, marginTop: 4, background: 'var(--kazakh-soft)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div className="title-serif" style={{ fontSize: 16 }}>Forest of Altai</div>
          <Label>★ key region</Label>
        </div>
        <Ornament variant="thin" style={{ margin: '6px 0' }} />
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 6 }}>
          <IconPlate label="forest" />
          <IconPlate label="yurt" faction="kazakh" />
          <IconPlate label="warr" />
          <IconPlate label="city" />
        </div>
        <div style={{ display: 'flex', gap: 8, marginTop: 6, flexWrap: 'wrap' }}>
          <Chip faction="kazakh">owner: Argyn</Chip>
          <Tag>warriors 3</Tag>
          <Tag>move 2</Tag>
          <Label>+2 prestige</Label>
        </div>
      </div>
    </div>

    {/* Bottom strip — phase tracker */}
    <div style={{ position: 'absolute', bottom: 0, left: 0, right: 0, height: 50, background: 'var(--paper-2)',
      borderTop: '1.5px solid var(--ink)', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 14 }}>
      <Tag>phase timeline</Tag>
      {['REINFORCE', 'ACTION', 'MIGRATION'].map((p, i) => (
        <div key={p} className="box" style={{ padding: '4px 12px',
          background: i === 0 ? 'var(--kazakh-soft)' : 'var(--paper)',
          boxShadow: i === 0 ? '0 0 0 2px rgba(185,137,42,0.5)' : 'none' }}>
          <span className="title-serif" style={{ fontSize: 13 }}>{p}</span>
          {i === 0 && <Tag style={{ marginLeft: 6 }}>active</Tag>}
        </div>
      ))}
      <span className="arrow">→</span>
      <Tag>contested tiles excluded from counts</Tag>
    </div>
  </div>
);

// Variation B: Sparse — minimal overlay, info-on-demand
const HudB = () => (
  <div className="wf tint-timurid" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper-2)' }} />
    <div style={{ position: 'absolute', left: 200, top: 200, display: 'grid', gridTemplateColumns: 'repeat(8, 56px)', gap: 4 }}>
      {Array.from({ length: 24 }).map((_, i) => {
        const f = ['timurid', null, 'kazakh', null, 'slavic', null, 'ming', null][i%8];
        return <HexMini key={i} faction={f} label={['✶','','◇','','✚','','☯',''][i%8]} />;
      })}
    </div>

    {/* Floating compact pill — top */}
    <div style={{ position: 'absolute', left: '50%', top: 18, transform: 'translateX(-50%)' }}>
      <div className="box thick" style={{ padding: '6px 14px', background: 'var(--paper)',
        display: 'flex', gap: 14, alignItems: 'center' }}>
        <Tag>turn</Tag>
        <span className="title-serif" style={{ fontSize: 18 }}>17</span>
        <div style={{ width: 1, height: 22, background: 'var(--ink)' }} />
        <Chip faction="timurid">Tamerlane</Chip>
        <div style={{ width: 1, height: 22, background: 'var(--ink)' }} />
        <span className="label" style={{ background: 'var(--timurid-soft)' }}>ACTION</span>
        <div style={{ width: 1, height: 22, background: 'var(--ink)' }} />
        <span className="title-serif" style={{ fontSize: 16 }}>9 / 15</span>
        <Tag>prestige</Tag>
      </div>
    </div>

    {/* Lone end-turn boss bottom-right */}
    <div style={{ position: 'absolute', right: 30, bottom: 30, textAlign: 'center' }}>
      <div style={{ width: 88, height: 88, borderRadius: '50%', border: '2.5px solid var(--ink)',
        background: 'var(--ming-soft)', display: 'grid', placeItems: 'center',
        boxShadow: '0 0 0 2px rgba(185,137,42,0.6), 0 0 22px rgba(185,137,42,0.4)' }}>
        <div className="title-serif" style={{ fontSize: 13, textAlign: 'center', lineHeight: 1.1 }}>END<br/>TURN</div>
      </div>
      <div style={{ display: 'flex', gap: 6, marginTop: 6, justifyContent: 'center' }}>
        <Label>AP 1/3</Label><Label>MP 2</Label>
      </div>
    </div>

    {/* Hover-only tooltip floating mid-map */}
    <div style={{ position: 'absolute', left: 360, top: 380, width: 240 }}>
      <Tag>summoned on hover</Tag>
      <div className="box" style={{ padding: 8, marginTop: 4, background: 'var(--timurid-soft)' }}>
        <div className="title-serif" style={{ fontSize: 14 }}>Plain · Samarqand</div>
        <Ornament variant="thin" style={{ margin: '4px 0' }} />
        <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
          <Chip faction="timurid">Tamerlane</Chip>
          <Tag>warr 4</Tag><Tag>move 1</Tag>
          <Label>★ key</Label>
        </div>
      </div>
    </div>

    {/* Slim left rail — phase + reinforce hint */}
    <div style={{ position: 'absolute', left: 14, top: 80, bottom: 80, width: 60,
      display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 10 }}>
      <Tag>phase rail</Tag>
      {['R','A','M'].map((p, i) => (
        <div key={p} style={{
          width: 44, height: 44, border: '1.5px solid var(--ink)', borderRadius: 4,
          background: i === 1 ? 'var(--timurid-soft)' : 'var(--paper)',
          display: 'grid', placeItems: 'center',
          boxShadow: i === 1 ? '0 0 0 2px rgba(185,137,42,0.5)' : 'none',
        }}>
          <span className="title-serif" style={{ fontSize: 18 }}>{p}</span>
        </div>
      ))}
      <Ornament variant="thin" style={{ width: 44 }} />
      <Annotate x={0} y={0} w={50} dir="" style={{ position: 'static', textAlign: 'center' }}>
        <span className="note" style={{ fontSize: 11, lineHeight: 1.1, display: 'block' }}>info<br/>on<br/>demand</span>
      </Annotate>
    </div>

    <Annotate x={650} y={28} w={300}>everything else hides until needed</Annotate>
  </div>
);

// Variation C: Rich Civ-like — full chrome, side empire stack, bottom dock
const HudC = () => (
  <div className="wf tint-slavic" style={{ width: 1280, height: 760, position: 'relative' }}>
    {/* Top: turn / phase / leader / prestige / end turn */}
    <div style={{ position: 'absolute', top: 0, left: 0, right: 0, height: 56, background: 'var(--paper-2)',
      borderBottom: '1.5px solid var(--ink)', display: 'grid', gridTemplateColumns: '120px 220px 1fr 280px 160px', gap: 8, padding: '6px 10px', alignItems: 'center' }}>
      <div className="box" style={{ padding: '4px 8px', background: 'var(--paper)' }}>
        <Tag>turn</Tag><div className="title-serif" style={{ fontSize: 18 }}>17 · 1360 AD</div>
      </div>
      <div className="box" style={{ padding: '4px 8px', background: 'var(--slavic-soft)', display: 'flex', gap: 6, alignItems: 'center' }}>
        <Placeholder faction="slavic" style={{ width: 36, height: 36, fontSize: 8 }}>p3</Placeholder>
        <div>
          <div className="title-serif" style={{ fontSize: 14 }}>Dmitry Donskoy</div>
          <Tag>Slavic Rus · player 3</Tag>
        </div>
      </div>
      <div style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
        {['REINFORCE', 'ACTION', 'MIGRATION'].map((p, i) => (
          <div key={p} className="box" style={{ padding: '4px 10px',
            background: i === 1 ? 'var(--slavic-soft)' : 'var(--paper)',
            boxShadow: i === 1 ? '0 0 0 2px rgba(185,137,42,0.55)' : 'none' }}>
            <span className="title-serif" style={{ fontSize: 12 }}>{p}</span>
          </div>
        ))}
        <span className="arrow">→</span>
        <Tag>auto-advance off</Tag>
      </div>
      <div className="box" style={{ padding: '4px 8px', background: 'var(--paper)', display: 'flex', alignItems: 'center', gap: 8 }}>
        <Tag>prestige</Tag>
        <div style={{ flex: 1, height: 10, background: 'var(--paper-2)', border: '1px solid var(--ink)', position: 'relative' }}>
          <div style={{ position: 'absolute', inset: 0, width: '40%', background: 'var(--slavic)' }} />
          {[0, 33, 66, 100].map(p => (
            <div key={p} style={{ position: 'absolute', left: `${p}%`, top: -2, bottom: -2, width: 1, background: 'var(--ink)' }} />
          ))}
        </div>
        <span className="title-serif" style={{ fontSize: 14 }}>6/15</span>
      </div>
      <div style={{ display: 'flex', gap: 6, alignItems: 'center', justifyContent: 'flex-end' }}>
        <Label>AP 2/3</Label><Label>MP 2</Label>
        <div style={{ width: 44, height: 44, borderRadius: '50%', border: '2px solid var(--ink)',
          background: 'var(--ming-soft)', display: 'grid', placeItems: 'center',
          fontFamily: 'Cormorant Garamond, serif', fontWeight: 700, fontSize: 10, textAlign: 'center', lineHeight: 1 }}>END<br/>TURN</div>
      </div>
    </div>

    {/* Map */}
    <div style={{ position: 'absolute', inset: '60px 240px 120px 0', background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper-2)' }}>
      <div style={{ position: 'absolute', left: 60, top: 50, display: 'grid', gridTemplateColumns: 'repeat(7, 56px)', gap: 4 }}>
        {Array.from({ length: 28 }).map((_, i) => {
          const f = ['slavic','slavic','kazakh',null,'ming',null,'timurid'][i%7];
          return <HexMini key={i} faction={f} label={['✚','✚','◇','','☯','','✶'][i%7]} />;
        })}
      </div>
    </div>

    {/* Right empire stack */}
    <div style={{ position: 'absolute', right: 0, top: 60, bottom: 120, width: 230,
      background: 'var(--paper-2)', borderLeft: '1.5px solid var(--ink)', padding: 10, overflow: 'hidden' }}>
      <Ornament variant="thin" />
      <div className="title-serif" style={{ fontSize: 15, margin: '6px 0' }}>Council of Empires</div>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        {[
          { f: 'kazakh', n: 'Argyn (Zhanibek)', p: 7, t: 14, y: 3, c: 1 },
          { f: 'timurid', n: 'Naiman (Tamerlane)', p: 9, t: 11, y: 2, c: 0 },
          { f: 'slavic', n: 'Rus (Donskoy)', p: 6, t: 13, y: 2, c: 1, active: true },
          { f: 'ming', n: 'Ming (Yongle)', p: 4, t: 8, y: 1, c: 0 },
        ].map((e, i) => (
          <div key={i} className="box" style={{ padding: 8, background: `var(--${e.f}-soft)`,
            boxShadow: e.active ? `inset 0 0 0 2px var(--${e.f})` : 'none' }}>
            <div style={{ display: 'grid', gridTemplateColumns: '32px 1fr auto', gap: 6, alignItems: 'center' }}>
              <Placeholder faction={e.f} style={{ height: 32, fontSize: 8 }}>·</Placeholder>
              <div>
                <div className="title-serif" style={{ fontSize: 13 }}>{e.n}</div>
                <div style={{ display: 'flex', gap: 4, marginTop: 2 }}>
                  <Tag>T{e.t}</Tag><Tag>Y{e.y}</Tag><Tag>C{e.c}</Tag>
                </div>
              </div>
              <span className="title-serif" style={{ fontSize: 14 }}>{e.p}/15</span>
            </div>
          </div>
        ))}
      </div>
      <Ornament variant="thin" style={{ marginTop: 8 }} />
      <Tag style={{ marginTop: 6, display: 'block' }}>+2 prestige per yurt / key city</Tag>
    </div>

    {/* Bottom dock — tooltip + reinforce calc + minimap */}
    <div style={{ position: 'absolute', left: 0, right: 0, bottom: 0, height: 110, background: 'var(--paper-2)',
      borderTop: '1.5px solid var(--ink)', display: 'grid', gridTemplateColumns: '320px 1fr 200px', gap: 10, padding: 10 }}>
      <div className="box" style={{ padding: 8, background: 'var(--slavic-soft)' }}>
        <Tag>tile · selected</Tag>
        <div className="title-serif" style={{ fontSize: 14 }}>Forest · Vladimir</div>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 4, marginTop: 4 }}>
          <IconPlate label="forest" size={28} />
          <IconPlate label="yurt" size={28} faction="slavic" />
          <IconPlate label="warr" size={28} />
          <IconPlate label="city" size={28} />
        </div>
        <div style={{ display: 'flex', gap: 6, marginTop: 4, flexWrap: 'wrap' }}>
          <Chip faction="slavic">Donskoy</Chip><Tag>warr 2</Tag><Tag>cost 2</Tag>
        </div>
      </div>
      <div className="box" style={{ padding: 8 }}>
        <Tag>action log · last 4</Tag>
        <div style={{ fontSize: 12, lineHeight: 1.4, marginTop: 4, fontFamily: 'Special Elite, monospace' }}>
          <div>17·1 ▸ Donskoy reinforced 5 (max(3, 13/3))</div>
          <div>17·2 ▸ Donskoy moved warriors → Vladimir [MP −2]</div>
          <div>17·3 ▸ contested at hex (4,2) — Tamerlane vs Donskoy</div>
          <div style={{ opacity: 0.5 }}>16·4 ▸ Argyn built yurt at Altai (+2 prestige)</div>
        </div>
      </div>
      <div className="box" style={{ padding: 6, background: 'var(--paper)' }}>
        <Tag>minimap</Tag>
        <div style={{ height: 80, marginTop: 4, background:
          'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 6px)',
          border: '1px solid var(--ink)', position: 'relative' }}>
          <div style={{ position: 'absolute', left: 8, top: 8, width: 24, height: 16, background: 'var(--slavic)', opacity: 0.5 }} />
          <div style={{ position: 'absolute', left: 50, top: 30, width: 20, height: 16, background: 'var(--kazakh)', opacity: 0.5 }} />
          <div style={{ position: 'absolute', right: 12, top: 14, width: 18, height: 14, background: 'var(--timurid)', opacity: 0.5 }} />
          <div style={{ position: 'absolute', right: 6, bottom: 6, width: 22, height: 16, background: 'var(--ming)', opacity: 0.5 }} />
        </div>
      </div>
    </div>
  </div>
);

Object.assign(window, { HudA, HudB, HudC });
