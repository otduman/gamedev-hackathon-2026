/* ====== Mid-fi shared primitives ====== */

const MfOrnament = ({ variant = 'line', style }) => (
  <div className={`orn-${variant}`} style={style} />
);

const MfPlaceholder = ({ children, faction, style, className = '' }) => (
  <div className={`mf-ph portrait-frame ${className}`} style={style}>
    {children}
  </div>
);

const MfChip = ({ faction, children }) => (
  <span className="mf-chip">
    <span className="dot" style={{ background: `var(--${faction})` }}></span>
    {children}
  </span>
);

const MfLabel = ({ children, style }) => (
  <span className="mf-label" style={style}>{children}</span>
);

const MfBtn = ({ felt, children, glow, style, sub }) => (
  <div className={`mf-btn tex-felt ${felt ? 'felt-' + felt : 'tex-leather'} ${glow ? 'glow pulse-gold' : ''}`} style={style}>
    {children}
    {sub && <span className="mf-label" style={{ fontSize: 9, letterSpacing: '0.04em' }}>{sub}</span>}
  </div>
);

const MfBar = ({ value, max, color, style }) => (
  <div className="mf-bar" style={style}>
    <div className="mf-bar-fill" style={{ width: `${(value / max) * 100}%`, background: color || 'var(--gold)' }} />
  </div>
);

const MfDie = ({ value, type = 'atk' }) => (
  <div className={`mf-die ${type}`}>{value}</div>
);

// Empire roster slot
const MfEmpireSlot = ({ faction, name, territories, warriors, active }) => (
  <div className={`mf-panel tex-felt felt-${faction}`} style={{
    padding: 8,
    boxShadow: active ? `inset 0 0 0 2px var(--${faction}), 0 0 14px rgba(200,168,78,0.3)` : 'none',
    transform: active ? 'scale(1.02)' : 'none',
    transition: 'all 0.3s',
  }}>
    <div style={{ display: 'grid', gridTemplateColumns: '32px 1fr auto', gap: 8, alignItems: 'center' }}>
      <MfPlaceholder faction={faction} style={{ width: 32, height: 32, fontSize: 8, border: `2px solid var(--${faction})` }}>·</MfPlaceholder>
      <div className="mf-title" style={{ fontSize: 12 }}>{name}</div>
      <div style={{ display: 'flex', gap: 10 }}>
        <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
          <span style={{ width: 11, height: 13, clipPath: 'polygon(50% 0, 100% 25%, 100% 75%, 50% 100%, 0 75%, 0 25%)',
            background: `var(--${faction})`, display: 'inline-block' }}></span>
          <span className="mf-title" style={{ fontSize: 13 }}>{territories}</span>
        </span>
        <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
          <span style={{ fontSize: 11, color: 'var(--gold-dim)' }}>⚔</span>
          <span className="mf-title" style={{ fontSize: 13 }}>{warriors}</span>
        </span>
      </div>
    </div>
  </div>
);

// Phase pill
const MfPhasePill = ({ label, icon, active }) => (
  <div className="mf-panel" style={{
    padding: '4px 10px', display: 'flex', alignItems: 'center', gap: 4,
    background: active ? 'rgba(200,168,78,0.15)' : 'transparent',
    borderColor: active ? 'var(--gold)' : 'rgba(200,168,78,0.2)',
    boxShadow: active ? '0 0 0 1px var(--gold), 0 0 14px rgba(200,168,78,0.3)' : 'none',
  }}>
    {icon && <span style={{ fontSize: 12, color: active ? 'var(--gold-bright)' : 'var(--gold-dim)' }}>{icon}</span>}
    <span className="mf-title" style={{ fontSize: 11, color: active ? 'var(--gold-bright)' : 'var(--gold-dim)' }}>{label}</span>
  </div>
);

// Secret mission parchment
const MfSecretMission = ({ text, style }) => (
  <div className="mf-panel tex-parchment" style={{
    padding: '8px 12px', borderStyle: 'dashed', borderColor: 'var(--brass)',
    background: 'var(--bg-parchment)', color: 'var(--ink)', ...style,
  }}>
    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
      <span style={{ fontSize: 18, color: 'var(--timurid)' }}>🝊</span>
      <div>
        <span className="mf-label" style={{ color: 'var(--ink-soft)', display: 'block' }}>Secret Mission</span>
        <span style={{ fontFamily: 'var(--font-body)', fontSize: 13, fontStyle: 'italic', color: 'var(--ink)' }}>{text}</span>
      </div>
    </div>
  </div>
);

// End phase boss button
const MfEndPhase = ({ size = 60 }) => (
  <div className="pulse-gold" style={{
    width: size, height: size, borderRadius: '50%', border: '3px solid var(--brass)',
    background: 'radial-gradient(circle at 40% 35%, var(--bg-leather) 0%, var(--bg-wood) 100%)',
    display: 'grid', placeItems: 'center', cursor: 'pointer',
    boxShadow: '0 0 0 2px rgba(200,168,78,0.4), 0 0 14px rgba(200,168,78,0.25), inset 0 1px 3px rgba(255,255,255,0.1)',
  }}>
    <div className="mf-title" style={{ fontSize: size > 50 ? 11 : 9, textAlign: 'center', lineHeight: 1.1 }}>END<br/>PHASE</div>
  </div>
);

Object.assign(window, {
  MfOrnament, MfPlaceholder, MfChip, MfLabel, MfBtn, MfBar, MfDie,
  MfEmpireSlot, MfPhasePill, MfSecretMission, MfEndPhase,
});
