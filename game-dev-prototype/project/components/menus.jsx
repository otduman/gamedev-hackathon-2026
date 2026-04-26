/* ============ MAIN MENU — 3 variations ============ */

// Variation A: Carved Plaque Centered — symmetrical heraldic
const MenuA = () => (
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    {/* sky/landscape placeholder */}
    <div style={{ position: 'absolute', inset: 0,
      background: 'linear-gradient(180deg, #d8c8a4 0%, #e6d4ad 35%, #c9d2b4 70%, #b9c19a 100%)' }} />
    {/* horizon silhouette */}
    <div style={{ position: 'absolute', left: 0, right: 0, top: 360, height: 60,
      background: 'repeating-linear-gradient(90deg, transparent 0 60px, rgba(29,26,22,0.18) 60px 62px)' }} />
    <Placeholder style={{ position: 'absolute', left: 40, top: 80, width: 240, height: 200, opacity: 0.85 }}>
      Steppe / mountains BG
    </Placeholder>
    <Placeholder style={{ position: 'absolute', right: 40, top: 80, width: 240, height: 200, opacity: 0.85 }}>
      Yurts • Wall • Fortress
    </Placeholder>

    {/* Title plaque */}
    <div style={{ position: 'absolute', left: '50%', top: 50, transform: 'translateX(-50%)', width: 720, textAlign: 'center' }}>
      <Ornament variant="weave" style={{ marginBottom: 8 }} />
      <div className="box thick" style={{ padding: '20px 30px', background: 'var(--paper-2)' }}>
        <Tag>plaque · carved wood + forged metal</Tag>
        <div className="title-serif scribble" style={{ fontSize: 56, letterSpacing: '0.08em', marginTop: 6 }}>
          RISE OF NOMADS
        </div>
        <div className="note" style={{ fontSize: 16, marginTop: 4 }}>— strategy of the great steppe —</div>
      </div>
      <Ornament variant="weave" style={{ marginTop: 8 }} />
    </div>

    {/* Center button stack */}
    <div style={{ position: 'absolute', left: '50%', top: 330, transform: 'translateX(-50%)',
      display: 'flex', flexDirection: 'column', gap: 14, width: 360 }}>
      <Btn felt="kazakh" hovered>▸ START GAME <Tag style={{ marginLeft: 6 }}>local hotseat</Tag></Btn>
      <Btn felt="slavic">OPTIONS</Btn>
      <Btn felt="timurid">QUIT</Btn>
    </div>

    {/* Left/Right leader portraits */}
    <div style={{ position: 'absolute', left: 60, bottom: 80, width: 200 }}>
      <div className="box double" style={{ padding: 8, background: 'var(--kazakh-soft)' }}>
        <Placeholder faction="kazakh" style={{ height: 180 }}>Leader portrait<br/>Kazakh</Placeholder>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 8 }}>
          <Chip faction="kazakh">Zhanibek</Chip>
          <Tag>tamga ◇</Tag>
        </div>
      </div>
      <Note style={{ display: 'block', textAlign: 'center', marginTop: 6 }}>brass frame · felt banner behind</Note>
    </div>
    <div style={{ position: 'absolute', right: 60, bottom: 80, width: 200 }}>
      <div className="box double" style={{ padding: 8, background: 'var(--timurid-soft)' }}>
        <Placeholder faction="timurid" style={{ height: 180 }}>Leader portrait<br/>Timurid</Placeholder>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 8 }}>
          <Chip faction="timurid">Tamerlane</Chip>
          <Tag>tamga ✶</Tag>
        </div>
      </div>
      <Note style={{ display: 'block', textAlign: 'center', marginTop: 6 }}>brass frame · felt banner behind</Note>
    </div>

    {/* footer */}
    <div style={{ position: 'absolute', left: 20, bottom: 14 }}><Tag>v0.1 · ALPHA</Tag></div>
    <div style={{ position: 'absolute', right: 20, bottom: 14 }}><Tag>© 2026 · NOMADS STUDIO</Tag></div>

    {/* annotation */}
    <Annotate x={510} y={300} w={260} dir="right">hover pulses warm gold</Annotate>
  </div>
);

// Variation B: Asymmetric — title left, scroll-style menu, leaders horizontal across bottom
const MenuB = () => (
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0,
      background: 'linear-gradient(180deg, #cdb997 0%, #d8c290 50%, #b6b58b 100%)' }} />
    <Placeholder style={{ position: 'absolute', inset: 0, opacity: 0.55, border: 'none' }}>
      Sunset steppe — full bleed BG
    </Placeholder>

    {/* Title left */}
    <div style={{ position: 'absolute', left: 60, top: 70, width: 520 }}>
      <Tag>plaque mounted on dark carved wood</Tag>
      <div className="box thick" style={{ padding: '24px 28px', marginTop: 6, background: 'var(--paper-2)' }}>
        <div className="title-serif" style={{ fontSize: 44, letterSpacing: '0.07em', lineHeight: 1.05 }}>
          RISE OF<br/>NOMADS
        </div>
        <Ornament style={{ marginTop: 12 }} />
        <div className="note" style={{ marginTop: 6 }}>a hex-grid empire game</div>
      </div>
    </div>

    {/* Menu list (scroll feel) */}
    <div style={{ position: 'absolute', right: 60, top: 90, width: 380 }}>
      <Tag>menu items render as leather strap rows</Tag>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 10, marginTop: 6 }}>
        {[
          { label: 'START GAME', sub: 'local hotseat', felt: 'kazakh', glyph: '▸', hov: true },
          { label: 'OPTIONS', sub: 'audio · video · controls', felt: 'slavic', glyph: '⚙' },
          { label: 'QUIT', sub: 'leave to desktop', felt: 'timurid', glyph: '⏻' },
        ].map((b, i) => (
          <div key={i} className={`box ${b.felt ? '' : ''}`} style={{
            padding: '12px 16px',
            background: `var(--${b.felt}-soft)`,
            display: 'grid', gridTemplateColumns: '32px 1fr auto', gap: 10, alignItems: 'center',
            boxShadow: b.hov ? '0 0 0 2px rgba(185,137,42,0.6)' : 'none',
          }}>
            <div className="label" style={{ textAlign: 'center', padding: '4px 0' }}>{b.glyph}</div>
            <div>
              <div className="title-serif" style={{ fontSize: 18 }}>{b.label}</div>
              <div className="note" style={{ fontSize: 12 }}>{b.sub}</div>
            </div>
            <div className="arrow">›</div>
          </div>
        ))}
      </div>
      <Annotate x={0} y={20} w={260} dir="left" style={{ position: 'relative', marginTop: 10 }}>
        active row glows gold + brass rivets pulse
      </Annotate>
    </div>

    {/* Leader strip across bottom */}
    <div style={{ position: 'absolute', left: 60, right: 60, bottom: 70 }}>
      <Tag>hotseat: tap to swap leader pair</Tag>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 14, marginTop: 6 }}>
        {[
          { f: 'kazakh', name: 'Zhanibek', emp: 'Kazakh', sym: '◇', active: true },
          { f: 'timurid', name: 'Tamerlane', emp: 'Timurid', sym: '✶', active: true },
          { f: 'slavic', name: 'Dmitry', emp: 'Rus', sym: '✚' },
          { f: 'ming', name: 'Yongle', emp: 'Ming', sym: '☯' },
        ].map((l, i) => (
          <div key={i} className="box" style={{
            padding: 8, background: `var(--${l.f}-soft)`,
            opacity: l.active ? 1 : 0.55,
          }}>
            <Placeholder faction={l.f} style={{ height: 110 }}>{l.emp}<br/>portrait</Placeholder>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 6 }}>
              <span className="title-serif" style={{ fontSize: 14 }}>{l.name}</span>
              <span className="label">{l.sym}</span>
            </div>
            {l.active && <div className="sticker" style={{ display: 'block', textAlign: 'center', marginTop: 4 }}>player {i+1}</div>}
          </div>
        ))}
      </div>
    </div>

    <div style={{ position: 'absolute', left: 20, bottom: 14 }}><Tag>v0.1 · ALPHA</Tag></div>
    <div style={{ position: 'absolute', right: 20, bottom: 14 }}><Tag>© 2026</Tag></div>
  </div>
);

// Variation C: Map-Table metaphor — title above a campaign map; menu lives on a leather-strap rail
const MenuC = () => (
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background: 'var(--paper-2)' }} />
    {/* Big map placeholder */}
    <div style={{ position: 'absolute', left: 200, top: 60, right: 60, bottom: 120 }}>
      <Tag>cartographic background — aged parchment map of Eurasia</Tag>
      <div className="box thick" style={{ position: 'relative', height: '95%', marginTop: 6, background:
        'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper)' }}>
        {/* sample hexes scattered */}
        <div style={{ position: 'absolute', left: 80, top: 60, display: 'grid', gridTemplateColumns: 'repeat(5, 56px)', gap: 4 }}>
          {['kazakh','slavic','timurid','ming','kazakh','slavic','kazakh','timurid','ming','kazakh'].map((f,i)=>(
            <HexMini key={i} faction={f} label={['◇','✚','✶','☯','◇','✚','◇','✶','☯','◇'][i]} />
          ))}
        </div>
        {/* scattered yurt/wall markers */}
        <Placeholder style={{ position: 'absolute', right: 60, top: 80, width: 90, height: 60 }}>yurts</Placeholder>
        <Placeholder style={{ position: 'absolute', right: 180, bottom: 60, width: 90, height: 60 }}>great wall</Placeholder>
        <Placeholder style={{ position: 'absolute', left: 300, bottom: 80, width: 90, height: 60 }}>fortress</Placeholder>

        {/* Title overlay floating on map */}
        <div style={{ position: 'absolute', left: '50%', top: 30, transform: 'translateX(-50%)', textAlign: 'center', width: 540 }}>
          <div className="box thick" style={{ padding: '12px 24px', background: 'var(--paper-2)' }}>
            <div className="title-serif scribble" style={{ fontSize: 42, letterSpacing: '0.08em' }}>RISE OF NOMADS</div>
            <Ornament style={{ marginTop: 6 }} />
          </div>
        </div>
      </div>
    </div>

    {/* Left leather rail with menu */}
    <div style={{ position: 'absolute', left: 30, top: 60, bottom: 120, width: 150 }}>
      <Tag>vertical leather strap rail</Tag>
      <div className="box thick" style={{ height: 'calc(100% - 22px)', marginTop: 6, padding: 10, background: 'var(--paper-2)',
        backgroundImage: 'repeating-linear-gradient(180deg, rgba(0,0,0,0.05) 0 6px, transparent 6px 14px)' }}>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <Btn felt="kazakh" hovered style={{ flexDirection: 'column', padding: 12 }}>
            <span style={{ fontSize: 12 }}>START</span>
            <span style={{ fontSize: 18 }}>GAME</span>
          </Btn>
          <Btn felt="slavic" style={{ padding: 12 }}>OPTIONS</Btn>
          <Btn felt="ming" style={{ padding: 12 }}>CREDITS</Btn>
          <Btn felt="timurid" style={{ padding: 12 }}>QUIT</Btn>
        </div>
        <Ornament variant="thin" style={{ marginTop: 14 }} />
        <Tag style={{ display: 'block', textAlign: 'center', marginTop: 10 }}>brass rivets at corners</Tag>
      </div>
    </div>

    {/* Bottom leader pair — selected */}
    <div style={{ position: 'absolute', left: 200, right: 60, bottom: 24, display: 'grid', gridTemplateColumns: '1fr auto 1fr', gap: 14, alignItems: 'center' }}>
      <div className="box" style={{ padding: 8, background: 'var(--kazakh-soft)', display: 'grid', gridTemplateColumns: '60px 1fr', gap: 10, alignItems: 'center' }}>
        <Placeholder faction="kazakh" style={{ height: 60, fontSize: 9 }}>P1</Placeholder>
        <div>
          <Chip faction="kazakh">Zhanibek · Kazakh</Chip>
          <div className="note" style={{ fontSize: 12, marginTop: 4 }}>Steppe horsemen · +1 MP plain</div>
        </div>
      </div>
      <div className="title-serif" style={{ fontSize: 22 }}>VS</div>
      <div className="box" style={{ padding: 8, background: 'var(--timurid-soft)', display: 'grid', gridTemplateColumns: '1fr 60px', gap: 10, alignItems: 'center' }}>
        <div style={{ textAlign: 'right' }}>
          <Chip faction="timurid">Tamerlane · Timurid</Chip>
          <div className="note" style={{ fontSize: 12, marginTop: 4 }}>Siege specialists · +1 atk forest</div>
        </div>
        <Placeholder faction="timurid" style={{ height: 60, fontSize: 9 }}>P2</Placeholder>
      </div>
    </div>

    <div style={{ position: 'absolute', left: 20, bottom: 6 }}><Tag>v0.1 · ALPHA</Tag></div>
    <div style={{ position: 'absolute', right: 20, bottom: 6 }}><Tag>© 2026</Tag></div>
  </div>
);

Object.assign(window, { MenuA, MenuB, MenuC });
