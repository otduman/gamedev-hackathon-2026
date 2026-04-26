/* ====== Mid-fi: Menu B — Asymmetric, leader strip ====== */

const MfMenuB = () => (
  <div className="tex-wood" style={{ width: 1280, height: 760, position: 'relative', overflow: 'hidden' }}>
    {/* Background landscape placeholder */}
    <div style={{ position: 'absolute', inset: 0,
      background: 'linear-gradient(180deg, #1a2a1a 0%, #2a3a1a 30%, #3a4a2a 60%, #2a3a1a 100%)',
      opacity: 0.6 }} />
    <MfPlaceholder style={{ position: 'absolute', inset: 0, border: 'none', opacity: 0.25, borderRadius: 0 }}>
      Sunset steppe panorama · mountains · yurts
    </MfPlaceholder>

    {/* Vignette overlay */}
    <div style={{ position: 'absolute', inset: 0,
      background: 'radial-gradient(ellipse at center, transparent 40%, rgba(0,0,0,0.6) 100%)',
      pointerEvents: 'none' }} />

    {/* Title block — left */}
    <div style={{ position: 'absolute', left: 60, top: 60, width: 520, zIndex: 1 }}>
      <MfOrnament variant="weave" style={{ marginBottom: 10 }} />
      <div className="mf-panel tex-wood" style={{ padding: '24px 30px', background: 'var(--bg-wood)',
        boxShadow: '0 8px 30px rgba(0,0,0,0.5), inset 0 1px 0 rgba(255,255,255,0.05)' }}>
        <MfLabel>— forged in the fires of the steppe —</MfLabel>
        <div className="mf-title" style={{ fontFamily: 'var(--font-title)', fontSize: 48, letterSpacing: '0.08em', marginTop: 8, lineHeight: 1.05,
          textShadow: '0 2px 8px rgba(0,0,0,0.6)' }}>
          RISE OF<br/>NOMADS
        </div>
        <MfOrnament variant="dots" style={{ marginTop: 12 }} />
        <div className="mf-body" style={{ marginTop: 8, fontSize: 15, fontStyle: 'italic', color: 'var(--gold-dim)' }}>
          A hex-grid empire game of conquest and glory
        </div>
      </div>
      <MfOrnament variant="weave" style={{ marginTop: 10 }} />
    </div>

    {/* Menu list — right */}
    <div style={{ position: 'absolute', right: 60, top: 80, width: 400, zIndex: 1 }}>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
        {[
          { label: 'START GAME', sub: 'local hotseat · 2–4 players', felt: 'kazakh', icon: '▸', glow: true },
          { label: 'OPTIONS', sub: 'audio · video · controls', felt: 'slavic', icon: '⚙' },
          { label: 'QUIT', sub: 'leave to desktop', felt: 'timurid', icon: '⏻' },
        ].map((b, i) => (
          <div key={i} className={`mf-panel tex-felt felt-${b.felt}`} style={{
            padding: '14px 18px',
            display: 'grid', gridTemplateColumns: '36px 1fr auto', gap: 12, alignItems: 'center',
            borderColor: b.glow ? 'var(--gold)' : 'rgba(200,168,78,0.25)',
            boxShadow: b.glow ? '0 0 0 1px var(--gold), 0 0 18px rgba(200,168,78,0.3)' : '0 4px 12px rgba(0,0,0,0.3)',
            cursor: 'pointer', transition: 'all 0.3s',
          }}>
            <div style={{
              width: 36, height: 36, borderRadius: 4, border: '1.5px solid var(--brass)',
              background: 'rgba(0,0,0,0.3)', display: 'grid', placeItems: 'center',
              fontFamily: 'var(--font-heading)', fontSize: 18, color: 'var(--gold)',
            }}>{b.icon}</div>
            <div>
              <div className="mf-title" style={{ fontSize: 17 }}>{b.label}</div>
              <div className="mf-body" style={{ fontSize: 12, color: 'var(--gold-dim)', marginTop: 2 }}>{b.sub}</div>
            </div>
            <span style={{ color: 'var(--gold-dim)', fontSize: 20, fontFamily: 'var(--font-heading)' }}>›</span>
          </div>
        ))}
      </div>
    </div>

    {/* Leader strip across bottom */}
    <div style={{ position: 'absolute', left: 60, right: 60, bottom: 50, zIndex: 1 }}>
      <MfOrnament variant="line" style={{ marginBottom: 10 }} />
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 14 }}>
        {[
          { f: 'kazakh', name: 'Zhanibek Khan', emp: 'Kazakh', sym: '◇', active: true },
          { f: 'timurid', name: 'Tamerlane', emp: 'Timurid', sym: '✶', active: true },
          { f: 'slavic', name: 'Dmitry Donskoy', emp: 'Slavic Rus', sym: '✚' },
          { f: 'ming', name: 'Emperor Yongle', emp: 'Ming Dynasty', sym: '☯' },
        ].map((l, i) => (
          <div key={i} className={`mf-panel tex-felt felt-${l.f}`} style={{
            padding: 10, opacity: l.active ? 1 : 0.5,
            boxShadow: l.active ? `0 0 0 1px var(--${l.f}), 0 0 12px rgba(200,168,78,0.2)` : '0 2px 8px rgba(0,0,0,0.3)',
            borderColor: l.active ? `var(--${l.f})` : 'rgba(200,168,78,0.15)',
          }}>
            <MfPlaceholder faction={l.f} style={{ height: 100 }}>{l.emp} portrait</MfPlaceholder>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 8 }}>
              <div>
                <div className="mf-title" style={{ fontSize: 13 }}>{l.name}</div>
                <MfLabel>{l.emp}</MfLabel>
              </div>
              <div style={{
                width: 24, height: 24, border: '1.5px solid var(--brass)', borderRadius: 3,
                display: 'grid', placeItems: 'center', fontFamily: 'var(--font-heading)',
                fontSize: 14, color: 'var(--gold)', background: 'rgba(0,0,0,0.3)',
              }}>{l.sym}</div>
            </div>
            {l.active && (
              <div style={{
                marginTop: 6, padding: '2px 8px', border: '1px dashed var(--gold-dim)', borderRadius: 3,
                fontFamily: 'var(--font-label)', fontSize: 9, color: 'var(--gold)', textAlign: 'center',
                textTransform: 'uppercase', letterSpacing: '0.1em',
              }}>player {i + 1}</div>
            )}
          </div>
        ))}
      </div>
    </div>

    {/* Footer */}
    <div style={{ position: 'absolute', left: 20, bottom: 14, zIndex: 1 }}>
      <MfLabel>v0.1 · alpha</MfLabel>
    </div>
    <div style={{ position: 'absolute', right: 20, bottom: 14, zIndex: 1 }}>
      <MfLabel>© 2026 nomads studio</MfLabel>
    </div>
  </div>
);

Object.assign(window, { MfMenuB });
