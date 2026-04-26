/* ============ Combat, Tooltip, Game-Over, Hotseat Select ============ */

// ===== Combat overlay =====
const CombatA = () => ( // Two physical mats sliding from sides
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    {/* dim background */}
    <div style={{ position: 'absolute', inset: 0, background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.06) 0 2px, transparent 2px 14px), var(--paper-2)', opacity: 0.7 }} />
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(29,26,22,0.25)' }} />

    {/* Attacker mat (left) */}
    <div style={{ position: 'absolute', left: 40, top: 130, width: 540, transform: 'rotate(-1.5deg)' }}>
      <Tag>red felt mat · slid in from left</Tag>
      <div className="box thick" style={{ padding: 16, marginTop: 4, background: 'var(--timurid-soft)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
            <Placeholder faction="timurid" style={{ width: 60, height: 60, fontSize: 9 }}>atk</Placeholder>
            <div>
              <div className="title-serif" style={{ fontSize: 18 }}>Tamerlane</div>
              <Chip faction="timurid">Timurid · attacker</Chip>
            </div>
          </div>
          <div className="title-serif" style={{ fontSize: 28 }}>3</div>
        </div>
        <Ornament variant="thin" style={{ margin: '12px 0' }} />
        <Tag>up to 3 red dice</Tag>
        <div style={{ display: 'flex', gap: 10, marginTop: 6, alignItems: 'center' }}>
          <div className="die atk">6</div><div className="die atk">5</div><div className="die atk">3</div>
          <div className="die atk" style={{ borderStyle: 'dashed', opacity: 0.7 }}>+1</div>
          <Label style={{ marginLeft: 4, background: 'var(--kazakh-soft)' }}>+ FOREST DIE</Label>
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 12 }}>
          <Tag>warriors 4</Tag><Tag>survivors: 2</Tag>
        </div>
      </div>
    </div>

    {/* Defender mat (right) */}
    <div style={{ position: 'absolute', right: 40, top: 230, width: 540, transform: 'rotate(1.2deg)' }}>
      <Tag>stone/white mat · slid in from right · ties to defender</Tag>
      <div className="box thick" style={{ padding: 16, marginTop: 4, background: 'var(--paper-2)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div className="title-serif" style={{ fontSize: 28 }}>2</div>
          <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
            <div style={{ textAlign: 'right' }}>
              <div className="title-serif" style={{ fontSize: 18 }}>Dmitry Donskoy</div>
              <Chip faction="slavic">Slavic · defender</Chip>
            </div>
            <Placeholder faction="slavic" style={{ width: 60, height: 60, fontSize: 9 }}>def</Placeholder>
          </div>
        </div>
        <Ornament variant="thin" style={{ margin: '12px 0' }} />
        <Tag>up to 2 stone dice</Tag>
        <div style={{ display: 'flex', gap: 10, marginTop: 6, alignItems: 'center' }}>
          <div className="die def">6</div><div className="die def">3</div>
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 12 }}>
          <Tag>warriors 3</Tag><Tag>survivors: 1</Tag>
        </div>
      </div>
    </div>

    {/* Result band bottom */}
    <div style={{ position: 'absolute', left: 40, right: 40, bottom: 30 }}>
      <div className="box thick" style={{ padding: 14, background: 'var(--paper)',
        display: 'grid', gridTemplateColumns: 'auto 1fr auto', gap: 16, alignItems: 'center' }}>
        <Label style={{ background: 'var(--timurid-soft)' }}>ATK 6 vs DEF 6 — defender wins</Label>
        <div className="title-serif" style={{ fontSize: 20, textAlign: 'center' }}>
          Tamerlane loses 1 · Donskoy loses 1 · tile remains <span className="scribble">CONTESTED</span>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          <Btn felt="kazakh">CONTINUE</Btn><Btn>RE-ROLL? <Tag>debug</Tag></Btn>
        </div>
      </div>
    </div>

    <Annotate x={520} y={70} w={240}>cinematic mats with ornament corners</Annotate>
  </div>
);

const CombatB = () => ( // Centered modal stacked
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(29,26,22,0.4)' }} />
    <div style={{ position: 'absolute', inset: 0, background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper-2)', opacity: 0.4 }} />

    <div style={{ position: 'absolute', left: '50%', top: '50%', transform: 'translate(-50%, -50%)', width: 620 }}>
      <Tag>centered modal · stacked attacker / defender</Tag>
      <div className="box thick" style={{ padding: 18, marginTop: 4, background: 'var(--paper)' }}>
        <Ornament style={{ marginBottom: 10 }} />
        <div className="title-serif" style={{ fontSize: 22, textAlign: 'center' }}>Battle for Vladimir</div>
        <div className="note" style={{ textAlign: 'center', marginTop: 2 }}>Risk-style · defender wins ties</div>
        <Ornament style={{ margin: '10px 0' }} />

        {/* Attacker row */}
        <div className="box" style={{ padding: 10, background: 'var(--timurid-soft)' }}>
          <div style={{ display: 'grid', gridTemplateColumns: '50px 1fr auto', gap: 10, alignItems: 'center' }}>
            <Placeholder faction="timurid" style={{ height: 50, fontSize: 9 }}>atk</Placeholder>
            <div>
              <div className="title-serif" style={{ fontSize: 16 }}>Tamerlane · Timurid</div>
              <div style={{ display: 'flex', gap: 6, marginTop: 4 }}>
                <div className="die atk">6</div><div className="die atk">5</div><div className="die atk">3</div>
                <div className="die atk" style={{ borderStyle: 'dashed' }}>+F</div>
              </div>
            </div>
            <div style={{ textAlign: 'center' }}>
              <Tag>warr</Tag><div className="title-serif" style={{ fontSize: 22 }}>4</div>
            </div>
          </div>
        </div>

        <div style={{ textAlign: 'center', margin: '8px 0', fontFamily: 'Cormorant Garamond, serif', fontWeight: 700, fontSize: 18 }}>
          ⚔ vs ⚔
        </div>

        {/* Defender row */}
        <div className="box" style={{ padding: 10, background: 'var(--slavic-soft)' }}>
          <div style={{ display: 'grid', gridTemplateColumns: 'auto 1fr 50px', gap: 10, alignItems: 'center' }}>
            <div style={{ textAlign: 'center' }}>
              <Tag>warr</Tag><div className="title-serif" style={{ fontSize: 22 }}>3</div>
            </div>
            <div>
              <div className="title-serif" style={{ fontSize: 16, textAlign: 'right' }}>Dmitry Donskoy · Rus</div>
              <div style={{ display: 'flex', gap: 6, marginTop: 4, justifyContent: 'flex-end' }}>
                <div className="die def">6</div><div className="die def">3</div>
              </div>
            </div>
            <Placeholder faction="slavic" style={{ height: 50, fontSize: 9 }}>def</Placeholder>
          </div>
        </div>

        <Ornament style={{ margin: '10px 0' }} />
        <div className="box" style={{ padding: 8, background: 'var(--paper-2)', textAlign: 'center' }}>
          <Tag>result</Tag>
          <div className="title-serif" style={{ fontSize: 16, marginTop: 2 }}>
            Tamerlane −1 · Donskoy −1 · <span className="scribble">tile contested</span>
          </div>
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 12, justifyContent: 'center' }}>
          <Btn felt="kazakh" hovered>RESOLVE</Btn>
          <Btn>RETREAT</Btn>
        </div>
      </div>
    </div>
  </div>
);

const CombatC = () => ( // Bottom drawer non-blocking
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    {/* map showing through */}
    <div style={{ position: 'absolute', inset: 0, background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper-2)' }} />
    <div style={{ position: 'absolute', left: 200, top: 100, display: 'grid', gridTemplateColumns: 'repeat(8, 56px)', gap: 4, opacity: 0.85 }}>
      {Array.from({ length: 24 }).map((_, i) => {
        const f = ['timurid','slavic',null,'kazakh',null,'ming','timurid','slavic'][i%8];
        return <HexMini key={i} faction={f} label="·" hatch={i === 10} />;
      })}
    </div>
    {/* highlighted contested hex */}
    <div style={{ position: 'absolute', left: 480, top: 220, padding: 8, border: '2px dashed var(--ink)', borderRadius: 8 }}>
      <Tag>contested hex</Tag>
    </div>

    {/* Bottom drawer */}
    <div style={{ position: 'absolute', left: 0, right: 0, bottom: 0, height: 240, background: 'var(--paper-2)',
      borderTop: '2.5px solid var(--ink)' }}>
      <Ornament style={{ position: 'absolute', left: 0, right: 0, top: -10 }} />
      <div style={{ padding: 14, display: 'grid', gridTemplateColumns: '1fr 80px 1fr 220px', gap: 14, height: '100%' }}>
        {/* Attacker */}
        <div className="box thick" style={{ padding: 12, background: 'var(--timurid-soft)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Placeholder faction="timurid" style={{ width: 46, height: 46, fontSize: 9 }}>atk</Placeholder>
            <div>
              <div className="title-serif" style={{ fontSize: 16 }}>Tamerlane</div>
              <Chip faction="timurid">attacker · 4 warr</Chip>
            </div>
          </div>
          <div style={{ display: 'flex', gap: 8, marginTop: 10 }}>
            <div className="die atk">6</div><div className="die atk">5</div><div className="die atk">3</div>
            <div className="die atk" style={{ borderStyle: 'dashed' }}>+F</div>
          </div>
          <Tag style={{ display: 'block', marginTop: 6 }}>+1 forest die · attacker in forest</Tag>
        </div>

        {/* VS column */}
        <div style={{ display: 'grid', placeItems: 'center', textAlign: 'center' }}>
          <div>
            <div className="title-serif" style={{ fontSize: 28 }}>VS</div>
            <Ornament variant="thin" style={{ margin: '6px 0' }} />
            <Tag>round 1</Tag>
          </div>
        </div>

        {/* Defender */}
        <div className="box thick" style={{ padding: 12, background: 'var(--slavic-soft)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Placeholder faction="slavic" style={{ width: 46, height: 46, fontSize: 9 }}>def</Placeholder>
            <div>
              <div className="title-serif" style={{ fontSize: 16 }}>Dmitry Donskoy</div>
              <Chip faction="slavic">defender · 3 warr</Chip>
            </div>
          </div>
          <div style={{ display: 'flex', gap: 8, marginTop: 10 }}>
            <div className="die def">6</div><div className="die def">3</div>
          </div>
          <Tag style={{ display: 'block', marginTop: 6 }}>defender wins ties</Tag>
        </div>

        {/* Result + actions */}
        <div className="box" style={{ padding: 10 }}>
          <Tag>result</Tag>
          <div className="title-serif" style={{ fontSize: 14, marginTop: 2 }}>−1 / −1 · contested</div>
          <Ornament variant="thin" style={{ margin: '6px 0' }} />
          <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
            <Btn felt="kazakh" hovered style={{ padding: '6px 10px', fontSize: 13 }}>ROLL AGAIN</Btn>
            <Btn style={{ padding: '6px 10px', fontSize: 13 }}>RETREAT</Btn>
            <Btn style={{ padding: '6px 10px', fontSize: 13 }}>CLOSE</Btn>
          </div>
        </div>
      </div>
    </div>

    <Annotate x={40} y={20} w={300}>map remains visible · drawer collapses to peek</Annotate>
  </div>
);

// ===== Tile tooltip =====
const TooltipA = () => ( // Compact felt panel
  <div className="wf" style={{ width: 720, height: 380, position: 'relative', padding: 30 }}>
    <Tag>compact · weathered felt · default</Tag>
    <div className="box thick" style={{ padding: 14, marginTop: 6, background: 'var(--kazakh-soft)', width: 320 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div className="title-serif" style={{ fontSize: 18 }}>Forest of Altai</div>
        <Label>★ KEY</Label>
      </div>
      <Ornament variant="thin" style={{ margin: '8px 0' }} />
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 6 }}>
        <IconPlate label="forest" />
        <IconPlate label="yurt" faction="kazakh" />
        <IconPlate label="warr" />
        <IconPlate label="city" />
      </div>
      <div style={{ display: 'flex', gap: 6, marginTop: 8, flexWrap: 'wrap' }}>
        <Chip faction="kazakh">Argyn</Chip>
        <Tag>warr 3</Tag>
        <Tag>move 2</Tag>
      </div>
    </div>
    <Annotate x={370} y={80} w={260} dir="left">terrain · owner · warr count · move cost · key flag</Annotate>
  </div>
);

const TooltipB = () => ( // Detailed expanded
  <div className="wf" style={{ width: 720, height: 480, position: 'relative', padding: 30 }}>
    <Tag>detailed · expanded on shift-hover</Tag>
    <div className="box thick" style={{ padding: 14, marginTop: 6, background: 'var(--paper-2)', width: 360 }}>
      <div style={{ display: 'grid', gridTemplateColumns: '54px 1fr auto', gap: 10, alignItems: 'center' }}>
        <IconPlate label="forest" size={54} />
        <div>
          <div className="title-serif" style={{ fontSize: 18 }}>Forest of Altai</div>
          <div style={{ display: 'flex', gap: 6, marginTop: 2 }}>
            <Chip faction="kazakh">Argyn</Chip>
            <Tag>hex (4,7)</Tag>
          </div>
        </div>
        <Label>★ KEY</Label>
      </div>
      <Ornament variant="thin" style={{ margin: '10px 0' }} />
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
        <div><Tag>terrain</Tag><div className="title-serif" style={{ fontSize: 14 }}>Forest</div><div className="note" style={{ fontSize: 11 }}>move cost 2</div></div>
        <div><Tag>warriors</Tag><div className="title-serif" style={{ fontSize: 14 }}>3</div><div className="note" style={{ fontSize: 11 }}>idle</div></div>
        <div><Tag>structure</Tag><div className="title-serif" style={{ fontSize: 14 }}>Yurt</div><div className="note" style={{ fontSize: 11 }}>+2 prestige</div></div>
        <div><Tag>contested</Tag><div className="title-serif" style={{ fontSize: 14 }}>No</div><div className="note" style={{ fontSize: 11 }}>counts toward owned</div></div>
      </div>
      <Ornament variant="thin" style={{ margin: '10px 0' }} />
      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
        <Label>+1 atk if you attack from here</Label>
        <Label>+1 forest die for attackers</Label>
      </div>
    </div>
    <Annotate x={400} y={120} w={260} dir="left">richer panel reveals modifiers + hex coord</Annotate>
  </div>
);

const TooltipC = () => ( // Contested state — special
  <div className="wf" style={{ width: 720, height: 380, position: 'relative', padding: 30 }}>
    <Tag>contested · two-owner state</Tag>
    <div className="box thick" style={{ padding: 14, marginTop: 6, background: 'var(--paper)', width: 360, position: 'relative' }}>
      <div className="hatch" style={{ position: 'absolute', inset: 0, opacity: 0.25, pointerEvents: 'none' }} />
      <div style={{ position: 'relative' }}>
        <div className="title-serif" style={{ fontSize: 18 }}>Plain · Vladimir</div>
        <div style={{ display: 'flex', gap: 6, marginTop: 4 }}>
          <Chip faction="slavic">Donskoy 2</Chip>
          <span className="title-serif">vs</span>
          <Chip faction="timurid">Tamerlane 4</Chip>
          <Label style={{ marginLeft: 'auto', background: 'var(--timurid-soft)' }}>CONTESTED</Label>
        </div>
        <Ornament variant="thin" style={{ margin: '8px 0' }} />
        <div className="note ink" style={{ fontSize: 13 }}>
          ⚠ excluded from prestige & tile counts until resolved.
        </div>
        <div style={{ display: 'flex', gap: 6, marginTop: 8 }}>
          <IconPlate label="plain" />
          <IconPlate label="warr" faction="timurid" />
          <IconPlate label="warr" faction="slavic" />
          <IconPlate label="—" />
        </div>
      </div>
    </div>
    <Annotate x={400} y={120} w={260} dir="left">crosshatch fill · contested badge</Annotate>
  </div>
);

// ===== Game Over =====
const GameOverA = () => ( // Plaque centered
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(29,26,22,0.45)' }} />
    <Placeholder style={{ position: 'absolute', inset: 0, opacity: 0.25, border: 'none' }}>battle aftermath BG</Placeholder>

    <div style={{ position: 'absolute', left: '50%', top: '50%', transform: 'translate(-50%, -50%)', width: 720 }}>
      <Tag>wooden plaque · hammered metal frame</Tag>
      <div className="box thick" style={{ padding: 24, marginTop: 6, background: 'var(--paper-2)' }}>
        <Ornament variant="weave" />
        <div className="title-serif" style={{ fontSize: 18, textAlign: 'center', marginTop: 10, letterSpacing: '0.16em' }}>VICTORY</div>
        <div className="title-serif scribble" style={{ fontSize: 44, textAlign: 'center', letterSpacing: '0.06em' }}>
          ZHANIBEK KHAN
        </div>
        <div className="note" style={{ textAlign: 'center', fontSize: 16 }}>Argyn · first to 15 prestige</div>
        <Ornament variant="weave" style={{ marginTop: 10 }} />

        <div style={{ display: 'grid', gridTemplateColumns: '160px 1fr', gap: 18, marginTop: 18, alignItems: 'center' }}>
          <Placeholder faction="kazakh" style={{ height: 160 }}>winner<br/>portrait + tamga</Placeholder>
          <div>
            <Tag>final stats</Tag>
            <Ornament variant="thin" style={{ margin: '4px 0' }} />
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 6, fontFamily: 'Special Elite, monospace', fontSize: 13 }}>
              <div>prestige</div><div className="title-serif">15 / 15</div>
              <div>tiles owned</div><div className="title-serif">22</div>
              <div>yurts / cities</div><div className="title-serif">5 / 2</div>
              <div>battles won</div><div className="title-serif">9</div>
              <div>warriors lost</div><div className="title-serif">14</div>
              <div>turns to victory</div><div className="title-serif">23</div>
            </div>
          </div>
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 18, justifyContent: 'center' }}>
          <Btn felt="kazakh" hovered>PLAY AGAIN</Btn>
          <Btn felt="slavic">MAIN MENU</Btn>
          <Btn>VIEW MAP</Btn>
        </div>
      </div>
    </div>

    <Annotate x={40} y={40} w={300}>conquest pulse — gold glow ripples on plaque</Annotate>
  </div>
);

const GameOverB = () => ( // Scoreboard horizontal
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(29,26,22,0.35)' }} />
    <div style={{ position: 'absolute', left: 60, right: 60, top: 60, bottom: 60 }}>
      <Tag>horizontal scoreboard · all 4 empires</Tag>
      <div className="box thick" style={{ padding: 20, marginTop: 6, background: 'var(--paper-2)', height: 'calc(100% - 26px)' }}>
        <Ornament />
        <div className="title-serif" style={{ fontSize: 32, textAlign: 'center', marginTop: 10 }}>The Khural Decides</div>
        <div className="note" style={{ textAlign: 'center' }}>turn 23 · prestige threshold reached</div>
        <Ornament style={{ marginTop: 10 }} />

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 12, marginTop: 20 }}>
          {[
            { f: 'kazakh', n: 'Zhanibek-khan', emp: 'Argyn', p: 15, t: 22, y: 5, win: true },
            { f: 'timurid', n: 'Tamerlane', emp: 'Naiman', p: 12, t: 18, y: 3 },
            { f: 'slavic', n: 'Dmitry Donskoy', emp: 'Rus', p: 9, t: 14, y: 2 },
            { f: 'ming', n: 'Yongle', emp: 'Ming', p: 7, t: 10, y: 2 },
          ].map((e, i) => (
            <div key={i} className="box thick" style={{ padding: 12, background: `var(--${e.f}-soft)`,
              boxShadow: e.win ? '0 0 0 3px rgba(185,137,42,0.7), 0 0 18px rgba(185,137,42,0.4)' : 'none' }}>
              {e.win && <Label style={{ background: 'var(--ming-soft)' }}>★ WINNER</Label>}
              <Placeholder faction={e.f} style={{ height: 140, marginTop: 6 }}>{e.emp}<br/>portrait</Placeholder>
              <div className="title-serif" style={{ fontSize: 16, marginTop: 6 }}>{e.n}</div>
              <Tag>{e.emp}</Tag>
              <Ornament variant="thin" style={{ margin: '6px 0' }} />
              <div style={{ display: 'grid', gridTemplateColumns: '1fr auto', gap: 4, fontSize: 12, fontFamily: 'Special Elite, monospace' }}>
                <span>prestige</span><span className="title-serif">{e.p}/15</span>
                <span>tiles</span><span className="title-serif">{e.t}</span>
                <span>yurts</span><span className="title-serif">{e.y}</span>
              </div>
            </div>
          ))}
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 24, justifyContent: 'center' }}>
          <Btn felt="kazakh" hovered>REMATCH</Btn>
          <Btn felt="slavic">MAIN MENU</Btn>
          <Btn felt="ming">EXPORT REPLAY</Btn>
        </div>
      </div>
    </div>
  </div>
);

const GameOverC = () => ( // Cinematic split — winner left, stats timeline right
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(29,26,22,0.45)' }} />
    {/* Left winner panel */}
    <div style={{ position: 'absolute', left: 60, top: 60, bottom: 60, width: 480 }}>
      <div className="box thick" style={{ padding: 20, height: 'calc(100% - 0px)', background: 'var(--kazakh-soft)' }}>
        <Tag>winner · full bleed portrait</Tag>
        <Placeholder faction="kazakh" style={{ height: 360, marginTop: 6 }}>winner portrait</Placeholder>
        <Ornament variant="weave" style={{ marginTop: 12 }} />
        <div className="title-serif" style={{ fontSize: 36, marginTop: 10 }}>Zhanibek-khan</div>
        <Chip faction="kazakh">Argyn — first to 15 prestige</Chip>
        <Ornament variant="thin" style={{ margin: '12px 0' }} />
        <div style={{ display: 'flex', gap: 8 }}>
          <Btn felt="kazakh" hovered>PLAY AGAIN</Btn>
          <Btn>MAIN MENU</Btn>
        </div>
      </div>
    </div>

    {/* Right timeline */}
    <div style={{ position: 'absolute', right: 60, top: 60, bottom: 60, width: 600 }}>
      <div className="box thick" style={{ padding: 20, height: 'calc(100% - 0px)', background: 'var(--paper-2)' }}>
        <Tag>prestige timeline · 23 turns</Tag>
        <div className="title-serif" style={{ fontSize: 22, marginTop: 4 }}>The Long March</div>
        <Ornament variant="thin" style={{ margin: '10px 0' }} />

        {/* fake line chart */}
        <div className="box" style={{ position: 'relative', height: 220, padding: 10, background: 'var(--paper)' }}>
          {/* axis */}
          <div style={{ position: 'absolute', left: 30, right: 10, top: 10, bottom: 30, borderLeft: '1.5px solid var(--ink)', borderBottom: '1.5px solid var(--ink)' }} />
          {/* lines */}
          <svg viewBox="0 0 100 100" preserveAspectRatio="none" style={{ position: 'absolute', left: 30, right: 10, top: 10, bottom: 30, width: 'calc(100% - 40px)', height: 'calc(100% - 40px)' }}>
            <polyline fill="none" stroke="var(--kazakh)" strokeWidth="1.5" points="0,90 12,80 25,65 40,55 55,40 70,28 85,12 100,2" />
            <polyline fill="none" stroke="var(--timurid)" strokeWidth="1.5" strokeDasharray="3,2" points="0,92 14,88 28,72 42,60 56,55 72,38 86,30 100,22" />
            <polyline fill="none" stroke="var(--slavic)" strokeWidth="1.5" strokeDasharray="2,3" points="0,94 15,90 30,82 45,75 60,68 75,55 90,46 100,40" />
            <polyline fill="none" stroke="var(--ming)" strokeWidth="1.5" strokeDasharray="1,2" points="0,95 18,90 35,86 50,80 66,72 80,65 92,58 100,52" />
          </svg>
          <Tag style={{ position: 'absolute', left: 8, top: 8 }}>15</Tag>
          <Tag style={{ position: 'absolute', left: 8, bottom: 8 }}>0</Tag>
          <Tag style={{ position: 'absolute', right: 8, bottom: 8 }}>turn 23</Tag>
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 10, flexWrap: 'wrap' }}>
          <Chip faction="kazakh">Argyn ▬▬</Chip>
          <Chip faction="timurid">Naiman --</Chip>
          <Chip faction="slavic">Rus ··</Chip>
          <Chip faction="ming">Ming ·</Chip>
        </div>

        <Ornament variant="thin" style={{ margin: '12px 0' }} />
        <Tag>highlights</Tag>
        <ul style={{ margin: '6px 0 0 18px', padding: 0, fontSize: 13, fontFamily: 'Special Elite, monospace', lineHeight: 1.5 }}>
          <li>T7 — Argyn captures Altai (+2)</li>
          <li>T14 — Argyn vs Naiman: 4 contested rounds</li>
          <li>T20 — Naiman holds Samarqand against 3 attacks</li>
          <li>T23 — Argyn builds 5th yurt → 15 prestige</li>
        </ul>
      </div>
    </div>
  </div>
);

// ===== Hotseat Leader Select =====
const HotseatA = () => ( // Side by side picker
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <Placeholder style={{ position: 'absolute', inset: 0, opacity: 0.4, border: 'none' }}>steppe BG</Placeholder>
    <div style={{ position: 'absolute', left: '50%', top: 30, transform: 'translateX(-50%)', textAlign: 'center' }}>
      <Tag>hotseat · pick two leaders</Tag>
      <div className="title-serif" style={{ fontSize: 28, marginTop: 4 }}>Choose Your Khans</div>
    </div>

    <div style={{ position: 'absolute', left: 40, right: 40, top: 110, bottom: 90, display: 'grid', gridTemplateColumns: '1fr 80px 1fr', gap: 14 }}>
      {/* Player 1 picker */}
      <div className="box thick" style={{ padding: 14, background: 'var(--paper-2)' }}>
        <Tag>player 1 · green seat</Tag>
        <Ornament variant="thin" style={{ margin: '6px 0' }} />
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 10 }}>
          {[
            { f: 'kazakh', n: 'Zhanibek', emp: 'Argyn', sel: true },
            { f: 'kazakh', n: 'Erejep', emp: 'Naiman' },
            { f: 'slavic', n: 'Dmitry', emp: 'Rus' },
            { f: 'ming', n: 'Yongle', emp: 'Ming' },
          ].map((l, i) => (
            <div key={i} className="box" style={{ padding: 8, background: `var(--${l.f}-soft)`,
              boxShadow: l.sel ? '0 0 0 2px rgba(185,137,42,0.7), 0 0 16px rgba(185,137,42,0.35)' : 'none' }}>
              <Placeholder faction={l.f} style={{ height: 110 }}>{l.emp}</Placeholder>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 6 }}>
                <span className="title-serif" style={{ fontSize: 14 }}>{l.n}</span>
                {l.sel && <Label>SELECTED</Label>}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* VS column */}
      <div style={{ display: 'grid', placeItems: 'center' }}>
        <div style={{ textAlign: 'center' }}>
          <div className="title-serif" style={{ fontSize: 36 }}>VS</div>
          <Ornament variant="thin" style={{ margin: '10px 0', width: 60 }} />
          <Tag>swap</Tag>
        </div>
      </div>

      {/* Player 2 picker */}
      <div className="box thick" style={{ padding: 14, background: 'var(--paper-2)' }}>
        <Tag>player 2 · red seat</Tag>
        <Ornament variant="thin" style={{ margin: '6px 0' }} />
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 10 }}>
          {[
            { f: 'kazakh', n: 'Zhanibek', emp: 'Argyn', dis: true },
            { f: 'timurid', n: 'Tamerlane', emp: 'Naiman', sel: true },
            { f: 'slavic', n: 'Dmitry', emp: 'Rus' },
            { f: 'ming', n: 'Yongle', emp: 'Ming' },
          ].map((l, i) => (
            <div key={i} className="box" style={{ padding: 8, background: `var(--${l.f}-soft)`,
              opacity: l.dis ? 0.4 : 1,
              boxShadow: l.sel ? '0 0 0 2px rgba(185,137,42,0.7), 0 0 16px rgba(185,137,42,0.35)' : 'none' }}>
              <Placeholder faction={l.f} style={{ height: 110 }}>{l.emp}</Placeholder>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 6 }}>
                <span className="title-serif" style={{ fontSize: 14 }}>{l.n}</span>
                {l.sel && <Label>SELECTED</Label>}
                {l.dis && <Tag>taken</Tag>}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>

    <div style={{ position: 'absolute', left: '50%', bottom: 24, transform: 'translateX(-50%)', display: 'flex', gap: 10 }}>
      <Btn>← BACK</Btn>
      <Btn felt="kazakh" hovered>BEGIN CAMPAIGN →</Btn>
    </div>
  </div>
);

const HotseatB = () => ( // Carousel
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <Placeholder style={{ position: 'absolute', inset: 0, opacity: 0.35, border: 'none' }}>steppe BG</Placeholder>

    <div style={{ position: 'absolute', left: '50%', top: 30, transform: 'translateX(-50%)', textAlign: 'center' }}>
      <Tag>carousel · scroll empires</Tag>
      <div className="title-serif" style={{ fontSize: 28, marginTop: 4 }}>The Four Empires</div>
    </div>

    {/* Big center card */}
    <div style={{ position: 'absolute', left: '50%', top: 130, transform: 'translateX(-50%)', width: 460 }}>
      <div className="box thick" style={{ padding: 16, background: 'var(--kazakh-soft)' }}>
        <Placeholder faction="kazakh" style={{ height: 240 }}>Zhanibek · Argyn</Placeholder>
        <Ornament variant="thin" style={{ margin: '10px 0' }} />
        <div className="title-serif" style={{ fontSize: 24 }}>Zhanibek-khan</div>
        <Tag>Argyn · Kazakh</Tag>
        <div className="note" style={{ marginTop: 6, fontSize: 13 }}>
          “Steppe horsemen” — +1 MP on Plain & Pasture · starts with 2 yurts.
        </div>
        <div style={{ display: 'flex', gap: 6, marginTop: 8, flexWrap: 'wrap' }}>
          <Label>+1 MP plain</Label>
          <Label>+2 starting warriors</Label>
          <Label>tamga ◇</Label>
        </div>
      </div>
    </div>

    {/* Side peek cards */}
    <div className="box" style={{ position: 'absolute', left: 60, top: 200, width: 180, padding: 10,
      background: 'var(--timurid-soft)', opacity: 0.65, transform: 'rotate(-2deg)' }}>
      <Placeholder faction="timurid" style={{ height: 130 }}>Tamerlane</Placeholder>
      <div className="title-serif" style={{ fontSize: 14, marginTop: 4 }}>Tamerlane</div>
      <Tag>← prev</Tag>
    </div>
    <div className="box" style={{ position: 'absolute', right: 60, top: 200, width: 180, padding: 10,
      background: 'var(--slavic-soft)', opacity: 0.65, transform: 'rotate(2deg)' }}>
      <Placeholder faction="slavic" style={{ height: 130 }}>Donskoy</Placeholder>
      <div className="title-serif" style={{ fontSize: 14, marginTop: 4 }}>Dmitry Donskoy</div>
      <Tag>next →</Tag>
    </div>

    {/* Slot indicators */}
    <div style={{ position: 'absolute', left: '50%', bottom: 90, transform: 'translateX(-50%)', display: 'flex', gap: 14 }}>
      <div className="box" style={{ padding: 10, background: 'var(--kazakh-soft)', display: 'flex', gap: 10, alignItems: 'center', boxShadow: '0 0 0 2px rgba(185,137,42,0.6)' }}>
        <Tag>P1</Tag>
        <span className="title-serif" style={{ fontSize: 14 }}>Zhanibek</span>
        <Label>locked</Label>
      </div>
      <div className="box" style={{ padding: 10, background: 'var(--paper-2)', display: 'flex', gap: 10, alignItems: 'center' }}>
        <Tag>P2</Tag>
        <span className="note ink">choose…</span>
      </div>
    </div>

    <div style={{ position: 'absolute', left: '50%', bottom: 24, transform: 'translateX(-50%)', display: 'flex', gap: 10 }}>
      <Btn>← BACK</Btn>
      <Btn felt="kazakh" hovered>CONFIRM PAIR</Btn>
    </div>
  </div>
);

const HotseatC = () => ( // Pass-the-device handoff state
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(29,26,22,0.5)' }} />
    <Placeholder style={{ position: 'absolute', inset: 0, opacity: 0.18, border: 'none' }}>map dimmed</Placeholder>

    <div style={{ position: 'absolute', left: '50%', top: '50%', transform: 'translate(-50%, -50%)', width: 560, textAlign: 'center' }}>
      <Tag>hotseat handoff · between turns</Tag>
      <div className="box thick" style={{ padding: 28, marginTop: 6, background: 'var(--paper-2)' }}>
        <Ornament variant="weave" />
        <div className="title-serif" style={{ fontSize: 16, marginTop: 12, letterSpacing: '0.18em' }}>END OF TURN 17</div>
        <div className="title-serif scribble" style={{ fontSize: 30, marginTop: 4 }}>Pass to Player 2</div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr auto 1fr', gap: 10, marginTop: 18, alignItems: 'center' }}>
          {/* outgoing */}
          <div className="box" style={{ padding: 10, background: 'var(--kazakh-soft)', opacity: 0.55 }}>
            <Placeholder faction="kazakh" style={{ height: 80 }}>P1</Placeholder>
            <div className="title-serif" style={{ fontSize: 14, marginTop: 4 }}>Zhanibek</div>
            <Tag>just played</Tag>
          </div>
          <div className="arrow" style={{ fontSize: 28 }}>→</div>
          {/* incoming */}
          <div className="box thick" style={{ padding: 10, background: 'var(--timurid-soft)',
            boxShadow: '0 0 0 2px rgba(185,137,42,0.7), 0 0 18px rgba(185,137,42,0.35)' }}>
            <Placeholder faction="timurid" style={{ height: 80 }}>P2</Placeholder>
            <div className="title-serif" style={{ fontSize: 14, marginTop: 4 }}>Tamerlane</div>
            <Tag>your turn</Tag>
          </div>
        </div>

        <Ornament variant="thin" style={{ margin: '14px 0' }} />
        <div className="note ink" style={{ fontSize: 13 }}>
          board hidden until P2 confirms · prevents fog-of-war leakage in hotseat
        </div>
        <div style={{ display: 'flex', gap: 10, marginTop: 14, justifyContent: 'center' }}>
          <Btn>review last turn</Btn>
          <Btn felt="timurid" hovered>I AM TAMERLANE — REVEAL BOARD</Btn>
        </div>
      </div>
    </div>
  </div>
);

Object.assign(window, {
  CombatA, CombatB, CombatC,
  TooltipA, TooltipB, TooltipC,
  GameOverA, GameOverB, GameOverC,
  HotseatA, HotseatB, HotseatC,
});
