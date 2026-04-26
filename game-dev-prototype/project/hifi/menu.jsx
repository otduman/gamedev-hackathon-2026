/* ====== Hi-fi: Main Menu — Interactive ====== */

const HfMenu = ({ onStart, onOptions }) => {
  const [hovered, setHovered] = React.useState(null);
  const [showAnnotations, setShowAnnotations] = React.useState(false);

  // Toggle annotations with 'A' key
  React.useEffect(() => {
    const handler = (e) => { if (e.key === 'a' || e.key === 'A') setShowAnnotations(v => !v); };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, []);

  return (
    <div className="t-wood-dark" style={{ width: 1920, height: 1080, position: 'relative', overflow: 'hidden' }}>
      {/* Dark atmospheric bg texture */}
      <div style={{ position: 'absolute', inset: 0,
        background: `
          radial-gradient(ellipse at 30% 60%, rgba(58,138,62,0.06) 0%, transparent 50%),
          radial-gradient(ellipse at 70% 40%, rgba(200,168,78,0.04) 0%, transparent 50%),
          radial-gradient(ellipse at 50% 80%, rgba(200,64,46,0.03) 0%, transparent 40%),
          var(--bg-dark)
        `}} />
      <Embers count={12} />
      <Vignette />

      {/* Title block — left */}
      <div className="anim-fade" style={{ position: 'absolute', left: 80, top: 80, width: 640, zIndex: 2 }}>
        <Orn variant="weave" style={{ marginBottom: 12 }} />
        <Panel tex="t-wood" style={{ padding: '32px 40px',
          boxShadow: '0 10px 40px rgba(0,0,0,0.6), inset 0 1px 0 rgba(255,255,255,0.04)' }}>
          <HfLabel>— forged in the fires of the steppe —</HfLabel>
          <div style={{
            fontFamily: 'var(--font-title)', fontSize: 64, fontWeight: 900,
            letterSpacing: '0.08em', lineHeight: 1.05, marginTop: 10,
            color: 'var(--cream)',
            textShadow: '0 3px 12px rgba(0,0,0,0.7), 0 0 40px rgba(200,168,78,0.15)',
          }}>
            <span className="shimmer-text">RISE OF</span><br/>
            <span className="shimmer-text">NOMADS</span>
          </div>
          <Orn variant="dots" style={{ marginTop: 14 }} />
          <div style={{
            fontFamily: 'var(--font-body)', fontSize: 17, fontStyle: 'italic',
            color: 'var(--gold-dim)', marginTop: 10,
          }}>
            A hex-grid empire game of conquest and glory
          </div>
        </Panel>
        <Orn variant="weave" style={{ marginTop: 12 }} />

        {showAnnotations && <>
          <UiSpec text="Panel_TitlePlaque [Image+TMP]" style={{ top: -18, left: 0 }} />
          <SfxCue text="ambient_wind_loop" style={{ bottom: -18, left: 0 }} />
        </>}
      </div>

      {/* Menu buttons — right */}
      <div className="anim-fade anim-d3" style={{ position: 'absolute', right: 80, top: 100, width: 480, zIndex: 2 }}>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          {[
            { key: 'start', label: 'START GAME', sub: 'local hotseat · 2–4 players', felt: 'kazakh', icon: '▸', action: onStart },
            { key: 'options', label: 'OPTIONS', sub: 'audio · video · controls', felt: 'slavic', icon: '⚙', action: onOptions },
            { key: 'quit', label: 'QUIT', sub: 'leave to desktop', felt: 'timurid', icon: '⏻' },
          ].map((b, i) => (
            <div key={b.key}
              className={`t-felt felt-${b.felt} pointer transition anim-fade anim-d${i + 3}`}
              onMouseEnter={() => setHovered(b.key)}
              onMouseLeave={() => setHovered(null)}
              onClick={b.action}
              style={{
                padding: '16px 22px', borderRadius: 6,
                display: 'grid', gridTemplateColumns: '42px 1fr auto', gap: 14, alignItems: 'center',
                border: `1.5px solid ${hovered === b.key ? 'var(--gold)' : 'var(--brass-border)'}`,
                boxShadow: hovered === b.key
                  ? '0 0 0 1px var(--gold), 0 0 22px var(--gold-glow), 0 6px 20px rgba(0,0,0,0.4)'
                  : '0 4px 14px rgba(0,0,0,0.4)',
                transition: 'all 0.3s',
                position: 'relative',
              }}>
              <div style={{
                width: 42, height: 42, borderRadius: 6,
                border: '1.5px solid var(--brass)', background: 'rgba(0,0,0,0.35)',
                display: 'grid', placeItems: 'center',
                fontFamily: 'var(--font-heading)', fontSize: 20,
                color: hovered === b.key ? 'var(--gold-bright)' : 'var(--gold)',
                transition: 'color 0.3s',
              }}>{b.icon}</div>
              <div>
                <HfTitle size={18}>{b.label}</HfTitle>
                <div style={{ fontFamily: 'var(--font-body)', fontSize: 13, color: 'var(--gold-dim)', marginTop: 2 }}>{b.sub}</div>
              </div>
              <span style={{
                color: hovered === b.key ? 'var(--gold-bright)' : 'var(--gold-dim)',
                fontSize: 22, fontFamily: 'var(--font-heading)', transition: 'color 0.3s',
              }}>›</span>

              {showAnnotations && i === 0 && (
                <UiSpec text={`Btn_${b.label.replace(' ', '')} [Button+TMP]`} style={{ top: -14, right: 0 }} />
              )}
            </div>
          ))}
        </div>

        {showAnnotations && <SfxCue text="ui_hover_leather_creak" style={{ bottom: -18, right: 0 }} />}
      </div>

      {/* Leader strip — bottom */}
      <div className="anim-slide-up anim-d5" style={{ position: 'absolute', left: 80, right: 80, bottom: 60, zIndex: 2 }}>
        <Orn style={{ marginBottom: 12 }} />
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 16 }}>
          {['kazakh', 'timurid', 'slavic', 'ming'].map((f, i) => {
            const l = LEADERS[f];
            const active = i < 2;
            return (
              <div key={f}
                className={`t-felt felt-${f} pointer transition anim-fade anim-d${i + 5}`}
                onMouseEnter={() => setHovered(f)}
                onMouseLeave={() => setHovered(null)}
                style={{
                  padding: 12, borderRadius: 6,
                  opacity: active ? 1 : 0.5,
                  border: `1.5px solid ${hovered === f || active ? `var(--${f})` : 'var(--brass-border)'}`,
                  boxShadow: active
                    ? `0 0 0 1px var(--${f}), 0 0 14px var(--${f}-glow), 0 4px 16px rgba(0,0,0,0.4)`
                    : '0 4px 12px rgba(0,0,0,0.3)',
                  transition: 'all 0.3s',
                  position: 'relative',
                }}>
                {/* Portrait */}
                <div style={{ width: '100%', height: 140, borderRadius: 4, overflow: 'hidden',
                  border: '2px solid var(--brass)', marginBottom: 10 }}>
                  <img src={l.img} alt={l.name} style={{
                    width: '100%', height: '100%', objectFit: 'cover', objectPosition: 'top center',
                    filter: active ? 'none' : 'grayscale(0.6) brightness(0.7)',
                    transition: 'filter 0.3s',
                  }} />
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <div>
                    <HfTitle size={14}>{l.name}</HfTitle>
                    <div><HfLabel>{l.empire}</HfLabel></div>
                  </div>
                  <div style={{
                    width: 28, height: 28, border: '1.5px solid var(--brass)', borderRadius: 4,
                    display: 'grid', placeItems: 'center',
                    fontFamily: 'var(--font-heading)', fontSize: 16, color: 'var(--gold)',
                    background: 'rgba(0,0,0,0.35)',
                  }}>{l.sym}</div>
                </div>
                {active && (
                  <div style={{
                    marginTop: 8, padding: '3px 10px', border: '1px dashed var(--gold-dim)',
                    borderRadius: 3, fontFamily: 'var(--font-label)', fontSize: 9,
                    color: 'var(--gold)', textAlign: 'center',
                    textTransform: 'uppercase', letterSpacing: '0.1em',
                  }}>player {i + 1}</div>
                )}
              </div>
            );
          })}
        </div>

        {showAnnotations && (
          <UiSpec text="Panel_LeaderStrip [HorizontalLayoutGroup] → LeaderCard_Prefab × 4" style={{ bottom: -18, left: 0 }} />
        )}
      </div>

      {/* Footer */}
      <div style={{ position: 'absolute', left: 30, bottom: 16, zIndex: 2 }}>
        <HfLabel>v0.1 · alpha</HfLabel>
      </div>
      <div style={{ position: 'absolute', right: 30, bottom: 16, zIndex: 2 }}>
        <HfLabel>© 2026 nomads studio</HfLabel>
      </div>
      <div style={{ position: 'absolute', right: 30, bottom: 34, zIndex: 2 }}>
        <HfLabel style={{ opacity: 0.4 }}>press A to toggle annotations</HfLabel>
      </div>
    </div>
  );
};

Object.assign(window, { HfMenu });
