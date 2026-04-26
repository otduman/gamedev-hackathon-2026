/* ====== Hi-fi shared primitives ====== */

// Game data constants
const LEADERS = {
  kazakh: { name: 'Zhanibek Khan', empire: 'Kazakh', sym: '◇', img: 'hifi/assets/kazakh.png', color: 'kazakh' },
  slavic: { name: 'Dmitry Donskoy', empire: 'Slavic Rus', sym: '✚', img: 'hifi/assets/slavic.png', color: 'slavic' },
  timurid: { name: 'Tamerlane', empire: 'Timurid', sym: '✶', img: 'hifi/assets/timurid.png', color: 'timurid' },
  ming: { name: 'Emperor Yongle', empire: 'Ming Dynasty', sym: '☯', img: 'hifi/assets/ming.png', color: 'ming' },
};

const PHASES = [
  { id: 'setup', label: 'SETUP', icon: '⚑' },
  { id: 'reinforce', label: 'REINFORCE', icon: '+' },
  { id: 'attack', label: 'ATTACK', icon: '⚔' },
  { id: 'fortify', label: 'FORTIFY', icon: '↔' },
];

// === Ornament ===
const Orn = ({ variant = 'gold', style }) => (
  <div className={`orn orn-${variant}`} style={style} />
);

// === Portrait with brass frame ===
const Portrait = ({ faction, size = 60, style, glow }) => (
  <div style={{
    width: size, height: size, borderRadius: 4, overflow: 'hidden',
    border: `2.5px solid var(--brass)`,
    boxShadow: glow
      ? `0 0 0 1px rgba(0,0,0,0.4), 0 0 12px var(--${faction}-glow)`
      : '0 0 0 1px rgba(0,0,0,0.4), 0 2px 8px rgba(0,0,0,0.5)',
    flexShrink: 0, ...style,
  }}>
    <img src={LEADERS[faction]?.img} alt={faction}
      style={{ width: '100%', height: '100%', objectFit: 'cover', objectPosition: 'top center', display: 'block' }} />
  </div>
);

// === Panel (wood/leather/felt border) ===
const Panel = ({ children, tex = 't-leather', felt, style, glow, className = '' }) => (
  <div className={`${tex} t-felt ${felt ? 'felt-' + felt : ''} ${className}`} style={{
    border: '1px solid var(--brass-border)',
    borderRadius: 6, padding: 12, position: 'relative',
    boxShadow: glow
      ? '0 0 0 1px var(--gold), 0 0 18px var(--gold-glow), 0 4px 16px rgba(0,0,0,0.4)'
      : '0 4px 16px rgba(0,0,0,0.4)',
    ...style,
  }}>
    {children}
  </div>
);

// === Button ===
const HfBtn = ({ children, felt, glow, onClick, style, sub, disabled }) => {
  const [hover, setHover] = React.useState(false);
  return (
    <div
      className={`t-felt ${felt ? 'felt-' + felt : 't-leather'} pointer transition`}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      onClick={disabled ? undefined : onClick}
      style={{
        fontFamily: 'var(--font-heading)', fontWeight: 600, fontSize: 15,
        letterSpacing: '0.08em', textTransform: 'uppercase',
        padding: '12px 24px', border: '2px solid var(--brass)', borderRadius: 4,
        display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: 8,
        color: disabled ? 'var(--cream-dim)' : 'var(--cream)',
        opacity: disabled ? 0.5 : 1, cursor: disabled ? 'not-allowed' : 'pointer',
        boxShadow: (hover || glow) && !disabled
          ? '0 0 0 2px rgba(200,168,78,0.5), 0 0 20px var(--gold-glow)'
          : '0 2px 8px rgba(0,0,0,0.4)',
        borderColor: (hover || glow) && !disabled ? 'var(--gold)' : 'var(--brass)',
        position: 'relative', transition: 'all 0.3s', ...style,
      }}>
      {/* Rivet dots */}
      <span style={{ position: 'absolute', left: 8, top: '50%', transform: 'translateY(-50%)',
        width: 5, height: 5, borderRadius: '50%', background: 'var(--brass)', opacity: 0.7 }} />
      <span style={{ position: 'absolute', right: 8, top: '50%', transform: 'translateY(-50%)',
        width: 5, height: 5, borderRadius: '50%', background: 'var(--brass)', opacity: 0.7 }} />
      {children}
      {sub && <span style={{ fontFamily: 'var(--font-label)', fontSize: 9, color: 'var(--gold-dim)' }}>{sub}</span>}
    </div>
  );
};

// === Chip ===
const HfChip = ({ faction, children }) => (
  <span style={{
    display: 'inline-flex', alignItems: 'center', gap: 6,
    border: '1px solid var(--brass-border)', padding: '2px 10px', borderRadius: 999,
    fontFamily: 'var(--font-body)', fontSize: 13, color: 'var(--cream)',
  }}>
    <span style={{ width: 8, height: 8, borderRadius: '50%', background: `var(--${faction})` }} />
    {children}
  </span>
);

// === Label ===
const HfLabel = ({ children, style }) => (
  <span style={{
    fontFamily: 'var(--font-label)', fontSize: 10, letterSpacing: '0.08em',
    textTransform: 'uppercase', color: 'var(--gold-dim)', ...style,
  }}>{children}</span>
);

// === Title ===
const HfTitle = ({ children, size = 16, style }) => (
  <span className="mf-title" style={{
    fontFamily: 'var(--font-heading)', fontWeight: 700, fontSize: size,
    letterSpacing: '0.05em', color: 'var(--cream)', ...style,
  }}>{children}</span>
);

// === Die ===
const HfDie = ({ value, type = 'atk', animate, delay = 0 }) => (
  <div className={animate ? `anim-dice` : ''} style={{
    width: 48, height: 48, border: `2.5px solid ${type === 'atk' ? 'var(--timurid)' : 'var(--slavic)'}`,
    borderRadius: 8, display: 'inline-grid', placeItems: 'center',
    fontFamily: 'var(--font-heading)', fontWeight: 700, fontSize: 24, color: 'var(--cream)',
    background: type === 'atk' ? 'var(--timurid-bg)' : 'var(--slavic-bg)',
    boxShadow: '3px 3px 8px rgba(0,0,0,0.5), inset 0 1px 0 rgba(255,255,255,0.08)',
    animationDelay: animate ? `${delay}s` : undefined,
  }}>{value}</div>
);

// === Empire Slot ===
const HfEmpireSlot = ({ faction, territories, warriors, active, onClick }) => {
  const l = LEADERS[faction];
  const [hover, setHover] = React.useState(false);
  return (
    <div className={`t-felt felt-${faction} pointer transition`}
      onClick={onClick}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      style={{
        padding: 8, borderRadius: 6, border: '1px solid var(--brass-border)',
        boxShadow: active
          ? `inset 0 0 0 2px var(--${faction}), 0 0 14px var(--${faction}-glow)`
          : hover ? '0 0 0 1px var(--brass), 0 0 10px var(--gold-glow)' : '0 2px 6px rgba(0,0,0,0.3)',
        transform: active ? 'scale(1.02)' : 'none', transition: 'all 0.3s',
      }}>
      <div style={{ display: 'grid', gridTemplateColumns: '34px 1fr auto', gap: 8, alignItems: 'center' }}>
        <Portrait faction={faction} size={34} glow={active} />
        <div>
          <HfTitle size={12}>{l.name}</HfTitle>
          <div><HfLabel>{l.empire}</HfLabel></div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
            <span style={{ width: 12, height: 14, clipPath: 'polygon(50% 0,100% 25%,100% 75%,50% 100%,0 75%,0 25%)',
              background: `var(--${faction})`, display: 'inline-block' }} />
            <HfTitle size={13}>{territories}</HfTitle>
          </span>
          <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
            <span style={{ fontSize: 11, color: 'var(--gold-dim)' }}>⚔</span>
            <HfTitle size={13}>{warriors}</HfTitle>
          </span>
        </div>
      </div>
    </div>
  );
};

// === Phase Pill ===
const HfPhasePill = ({ phase, active, onClick }) => {
  const [hover, setHover] = React.useState(false);
  return (
    <div className="pointer transition"
      onClick={onClick}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      style={{
        padding: '5px 12px', borderRadius: 4, display: 'flex', alignItems: 'center', gap: 5,
        border: `1px solid ${active ? 'var(--gold)' : 'rgba(200,168,78,0.2)'}`,
        background: active ? 'rgba(200,168,78,0.15)' : hover ? 'rgba(200,168,78,0.06)' : 'transparent',
        boxShadow: active ? '0 0 0 1px var(--gold), 0 0 14px var(--gold-glow)' : 'none',
        transition: 'all 0.3s',
      }}>
      <span style={{ fontSize: 13, color: active ? 'var(--gold-bright)' : 'var(--gold-dim)' }}>{phase.icon}</span>
      <span style={{
        fontFamily: 'var(--font-heading)', fontWeight: 600, fontSize: 11,
        letterSpacing: '0.06em', color: active ? 'var(--gold-bright)' : 'var(--gold-dim)',
      }}>{phase.label}</span>
    </div>
  );
};

// === Secret Mission Parchment ===
const HfMission = ({ text, style }) => (
  <div className="t-parchment" style={{
    padding: '10px 14px', borderRadius: 4, border: '1.5px dashed var(--brass)',
    ...style,
  }}>
    <div style={{ display: 'flex', alignItems: 'flex-start', gap: 8 }}>
      <span style={{ fontSize: 18, color: 'var(--timurid)', flexShrink: 0, marginTop: 1 }}>🝊</span>
      <div style={{ minWidth: 0 }}>
        <span style={{ fontFamily: 'var(--font-label)', fontSize: 9, color: 'var(--ink-soft)',
          textTransform: 'uppercase', letterSpacing: '0.08em', display: 'block', marginBottom: 2 }}>Secret Mission</span>
        <span style={{ fontFamily: 'var(--font-body)', fontSize: 12, fontStyle: 'italic', color: 'var(--ink)',
          lineHeight: 1.3, display: 'block' }}>{text}</span>
      </div>
    </div>
  </div>
);

// === End Phase Boss Button ===
const HfEndPhase = ({ onClick, size = 60, active }) => {
  const [hover, setHover] = React.useState(false);
  return (
    <div className={`pointer ${active ? 'anim-pulse' : ''}`}
      onClick={onClick}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      style={{
        width: size, height: size, borderRadius: '50%', border: '3px solid var(--brass)',
        background: 'radial-gradient(circle at 40% 35%, var(--bg-leather) 0%, var(--bg-wood) 100%)',
        display: 'grid', placeItems: 'center', cursor: 'pointer',
        boxShadow: hover || active
          ? '0 0 0 3px rgba(200,168,78,0.5), 0 0 24px var(--gold-glow), inset 0 1px 3px rgba(255,255,255,0.1)'
          : '0 0 0 1px rgba(0,0,0,0.3), 0 2px 8px rgba(0,0,0,0.4), inset 0 1px 3px rgba(255,255,255,0.06)',
        transition: 'all 0.3s',
      }}>
      <HfTitle size={size > 50 ? 11 : 9} style={{ textAlign: 'center', lineHeight: 1.1 }}>END<br/>PHASE</HfTitle>
    </div>
  );
};

// === SFX Annotation ===
const SfxCue = ({ text, style }) => (
  <div style={{
    position: 'absolute', padding: '2px 8px', background: 'rgba(200,64,46,0.85)',
    borderRadius: 3, fontFamily: 'var(--font-label)', fontSize: 8,
    color: '#fff', textTransform: 'uppercase', letterSpacing: '0.1em',
    pointerEvents: 'none', whiteSpace: 'nowrap', zIndex: 100, ...style,
  }}>♪ SFX: {text}</div>
);

// === Unity Spec Annotation ===
const UiSpec = ({ text, style }) => (
  <div style={{
    position: 'absolute', padding: '2px 8px', background: 'rgba(42,106,170,0.85)',
    borderRadius: 3, fontFamily: 'monospace', fontSize: 8,
    color: '#fff', letterSpacing: '0.04em',
    pointerEvents: 'none', whiteSpace: 'nowrap', zIndex: 100, ...style,
  }}>⬡ {text}</div>
);

// === Ember particles ===
const Embers = ({ count = 8 }) => (
  <div style={{ position: 'absolute', inset: 0, pointerEvents: 'none', overflow: 'hidden', zIndex: 0 }}>
    {Array.from({ length: count }).map((_, i) => (
      <div key={i} style={{
        position: 'absolute',
        left: `${10 + Math.random() * 80}%`,
        bottom: `${Math.random() * 30}%`,
        width: 3 + Math.random() * 3,
        height: 3 + Math.random() * 3,
        borderRadius: '50%',
        background: `radial-gradient(circle, var(--gold-bright), var(--gold-dim))`,
        animation: `emberFloat ${3 + Math.random() * 4}s ease-out infinite`,
        animationDelay: `${Math.random() * 5}s`,
        opacity: 0,
      }} />
    ))}
  </div>
);

// === Vignette ===
const Vignette = () => (
  <div style={{
    position: 'absolute', inset: 0, pointerEvents: 'none', zIndex: 1,
    background: 'radial-gradient(ellipse at center, transparent 35%, rgba(0,0,0,0.6) 100%)',
  }} />
);

Object.assign(window, {
  LEADERS, PHASES,
  Orn, Portrait, Panel, HfBtn, HfChip, HfLabel, HfTitle, HfDie,
  HfEmpireSlot, HfPhasePill, HfMission, HfEndPhase, SfxCue, UiSpec,
  Embers, Vignette,
});
