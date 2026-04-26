/* ============ Shared wireframe primitives (React components) ============ */

const Ornament = ({ variant = 'dots', style }) => (
  <div className={`ornament ${variant === 'thin' ? 'thin' : variant === 'weave' ? 'weave' : ''}`} style={style} />
);

const Placeholder = ({ children, faction, style, className = '' }) => (
  <div className={`ph ${faction || ''} ${className}`} style={style}>
    <span>{children}</span>
  </div>
);

const Chip = ({ faction, children }) => (
  <span className={`chip ${faction || ''}`}>
    <span className="dot"></span>
    {children}
  </span>
);

const Tag = ({ children, style }) => (
  <span className="tag" style={style}>{children}</span>
);

const Note = ({ children, style, ink }) => (
  <span className={`note ${ink ? 'ink' : ''}`} style={style}>{children}</span>
);

const Label = ({ children, style }) => (
  <span className="label" style={style}>{children}</span>
);

const Btn = ({ felt, children, style, straps = true, hovered = false }) => (
  <div className={`btn ${felt ? 'felt-' + felt : ''}`}
       style={{ ...style, boxShadow: hovered ? '0 0 0 2px rgba(185,137,42,0.6), 0 0 18px rgba(185,137,42,0.45)' : 'none' }}>
    {straps && <span className="strap l"></span>}
    {straps && <span className="strap r"></span>}
    {children}
  </div>
);

// hex grid mini for context
const HexMini = ({ count = 7, faction, label, owner, hatch }) => (
  <div className="hex" style={{
    background: faction === 'kazakh' ? 'var(--kazakh-soft)' :
                faction === 'slavic' ? 'var(--slavic-soft)' :
                faction === 'timurid' ? 'var(--timurid-soft)' :
                faction === 'ming' ? 'var(--ming-soft)' : 'var(--paper-2)',
    backgroundImage: hatch ? 'repeating-linear-gradient(45deg, rgba(0,0,0,0.18) 0 1px, transparent 1px 5px)' : undefined,
    border: 'none',
  }}>
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', lineHeight: 1.1, gap: 1 }}>
      {label && <span style={{ fontWeight: 700, fontSize: 11 }}>{label}</span>}
      {owner && <span style={{ fontSize: 8, opacity: 0.7 }}>{owner}</span>}
    </div>
  </div>
);

// dashed annotation arrow
const Annotate = ({ children, x, y, w, dir = 'right', style }) => (
  <div style={{ position: 'absolute', left: x, top: y, width: w, ...style }}>
    <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
      {dir === 'left' && <span className="arrow">←</span>}
      <span className="note">{children}</span>
      {dir === 'right' && <span className="arrow">→</span>}
    </div>
  </div>
);

// section header inside an artboard
const SectionHead = ({ title, sub }) => (
  <div style={{ marginBottom: 10 }}>
    <div className="title-serif" style={{ fontSize: 18, letterSpacing: '0.05em' }}>{title}</div>
    {sub && <div className="note" style={{ fontSize: 13 }}>{sub}</div>}
  </div>
);

// cute icon glyph (stand-in for actual KayKit icons)
const Glyph = ({ ch = '✦', size = 14, style }) => (
  <span style={{
    display: 'inline-grid', placeItems: 'center',
    width: size + 8, height: size + 8,
    border: '1.2px solid var(--ink)', borderRadius: '50%',
    fontSize: size, fontFamily: 'Cormorant Garamond, serif',
    background: 'var(--paper)', ...style,
  }}>{ch}</span>
);

// rectangular icon plate (with sketchy cross)
const IconPlate = ({ label, size = 36, faction, style }) => (
  <div style={{
    width: size, height: size,
    border: '1.5px solid var(--ink)', borderRadius: 4,
    background: faction === 'kazakh' ? 'var(--kazakh-soft)' :
                faction === 'slavic' ? 'var(--slavic-soft)' :
                faction === 'timurid' ? 'var(--timurid-soft)' :
                faction === 'ming' ? 'var(--ming-soft)' : 'var(--paper-2)',
    display: 'grid', placeItems: 'center',
    fontFamily: 'Special Elite, monospace', fontSize: 9,
    color: 'var(--ink-2)', position: 'relative', ...style,
  }}>
    <span style={{ background: 'var(--paper)', padding: '0 3px', border: '1px solid var(--ink)', borderRadius: 2 }}>{label}</span>
  </div>
);

Object.assign(window, {
  Ornament, Placeholder, Chip, Tag, Note, Label, Btn, HexMini,
  Annotate, SectionHead, Glyph, IconPlate,
});
