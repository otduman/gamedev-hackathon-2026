/* ============ Combat v2, Tooltip v2 — Classic Risk ============ */

// ===== Combat: two mats from sides =====
const CombatA2 = () => (
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(29,26,22,0.3)' }} />
    <div style={{ position: 'absolute', inset: 0, background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper-2)', opacity: 0.5 }} />

    {/* Attacker mat left */}
    <div style={{ position: 'absolute', left: 40, top: 140, width: 520, transform: 'rotate(-1.2deg)' }}>
      <Tag>red felt mat · slid from left</Tag>
      <div className="box thick" style={{ padding: 16, marginTop: 4, background: 'var(--timurid-soft)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
            <Placeholder faction="timurid" style={{ width: 56, height: 56, fontSize: 9 }}>atk</Placeholder>
            <div>
              <div className="title-serif" style={{ fontSize: 17 }}>Tamerlane</div>
              <Chip faction="timurid">Timurid · attacker</Chip>
              <div className="note" style={{ fontSize: 12, marginTop: 2 }}>attacking from <span className="scribble">Samarqand</span></div>
            </div>
          </div>
          <div style={{ textAlign: 'center' }}>
            <Tag>warriors</Tag>
            <div className="title-serif" style={{ fontSize: 26 }}>5</div>
          </div>
        </div>
        <Ornament variant="thin" style={{ margin: '10px 0' }} />
        <Tag>up to 3 red wooden dice</Tag>
        <div style={{ display: 'flex', gap: 10, marginTop: 6 }}>
          <div className="die atk">6</div><div className="die atk">4</div><div className="die atk">2</div>
        </div>
      </div>
    </div>

    {/* Defender mat right */}
    <div style={{ position: 'absolute', right: 40, top: 240, width: 520, transform: 'rotate(1deg)' }}>
      <Tag>stone mat · slid from right · defender wins ties</Tag>
      <div className="box thick" style={{ padding: 16, marginTop: 4, background: 'var(--paper-2)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ textAlign: 'center' }}>
            <Tag>warriors</Tag>
            <div className="title-serif" style={{ fontSize: 26 }}>3</div>
          </div>
          <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
            <div style={{ textAlign: 'right' }}>
              <div className="title-serif" style={{ fontSize: 17 }}>Dmitry Donskoy</div>
              <Chip faction="slavic">Slavic · defender</Chip>
              <div className="note" style={{ fontSize: 12, marginTop: 2 }}>defending <span className="scribble">Kashgar</span></div>
            </div>
            <Placeholder faction="slavic" style={{ width: 56, height: 56, fontSize: 9 }}>def</Placeholder>
          </div>
        </div>
        <Ornament variant="thin" style={{ margin: '10px 0' }} />
        <Tag>up to 2 grey/blue dice</Tag>
        <div style={{ display: 'flex', gap: 10, marginTop: 6, justifyContent: 'flex-end' }}>
          <div className="die def">5</div><div className="die def">3</div>
        </div>
      </div>
    </div>

    {/* Result banner */}
    <div style={{ position: 'absolute', left: 60, right: 60, bottom: 40 }}>
      <div className="box thick" style={{ padding: 14, background: 'var(--paper)',
        display: 'grid', gridTemplateColumns: '1fr auto', gap: 16, alignItems: 'center' }}>
        <div>
          <Tag>result</Tag>
          <div className="title-serif" style={{ fontSize: 18, marginTop: 2 }}>
            6 vs 5 → attacker wins · 4 vs 3 → attacker wins
          </div>
          <div className="note ink" style={{ fontSize: 14, marginTop: 4 }}>
            Defender loses 2 warriors. Defender wins ties.
          </div>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          <Btn felt="timurid" hovered>ATTACK AGAIN</Btn>
          <Btn>STOP</Btn>
        </div>
      </div>
    </div>
  </div>
);

// ===== Combat: centered modal =====
const CombatB2 = () => (
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background: 'rgba(29,26,22,0.4)' }} />

    <div style={{ position: 'absolute', left: '50%', top: '50%', transform: 'translate(-50%, -50%)', width: 600 }}>
      <div className="box thick" style={{ padding: 18, background: 'var(--paper)' }}>
        <Ornament />
        <div className="title-serif" style={{ fontSize: 20, textAlign: 'center', marginTop: 8 }}>
          Samarqand → Kashgar
        </div>
        <div className="note" style={{ textAlign: 'center' }}>classic Risk · defender wins ties</div>
        <Ornament style={{ margin: '10px 0' }} />

        {/* Attacker */}
        <div className="box" style={{ padding: 10, background: 'var(--timurid-soft)' }}>
          <div style={{ display: 'grid', gridTemplateColumns: '46px 1fr auto', gap: 10, alignItems: 'center' }}>
            <Placeholder faction="timurid" style={{ height: 46, fontSize: 9 }}>atk</Placeholder>
            <div>
              <div className="title-serif" style={{ fontSize: 15 }}>Tamerlane · Timurid</div>
              <div className="note" style={{ fontSize: 12 }}>from Samarqand</div>
              <div style={{ display: 'flex', gap: 8, marginTop: 6 }}>
                <div className="die atk">6</div><div className="die atk">4</div><div className="die atk">2</div>
              </div>
            </div>
            <div style={{ textAlign: 'center' }}>
              <Tag>warr</Tag><div className="title-serif" style={{ fontSize: 22 }}>5</div>
            </div>
          </div>
        </div>

        <div style={{ textAlign: 'center', margin: '8px 0', fontFamily: 'Cormorant Garamond, serif', fontWeight: 700, fontSize: 18 }}>⚔</div>

        {/* Defender */}
        <div className="box" style={{ padding: 10, background: 'var(--slavic-soft)' }}>
          <div style={{ display: 'grid', gridTemplateColumns: 'auto 1fr 46px', gap: 10, alignItems: 'center' }}>
            <div style={{ textAlign: 'center' }}>
              <Tag>warr</Tag><div className="title-serif" style={{ fontSize: 22 }}>3</div>
            </div>
            <div style={{ textAlign: 'right' }}>
              <div className="title-serif" style={{ fontSize: 15 }}>Dmitry Donskoy · Rus</div>
              <div className="note" style={{ fontSize: 12 }}>defending Kashgar</div>
              <div style={{ display: 'flex', gap: 8, marginTop: 6, justifyContent: 'flex-end' }}>
                <div className="die def">5</div><div className="die def">3</div>
              </div>
            </div>
            <Placeholder faction="slavic" style={{ height: 46, fontSize: 9 }}>def</Placeholder>
          </div>
        </div>

        <Ornament style={{ margin: '10px 0' }} />
        <div className="box" style={{ padding: 8, background: 'var(--paper-2)', textAlign: 'center' }}>
          <div className="title-serif" style={{ fontSize: 15 }}>
            Defender loses 2 · Attacker loses 0
          </div>
          <div className="note" style={{ fontSize: 12, marginTop: 2 }}>defender wins ties</div>
        </div>

        <div style={{ display: 'flex', gap: 10, marginTop: 14, justifyContent: 'center' }}>
          <Btn felt="timurid" hovered>ATTACK AGAIN</Btn>
          <Btn>STOP ATTACKING</Btn>
        </div>
      </div>
    </div>
  </div>
);

// ===== Combat: bottom drawer =====
const CombatC2 = () => (
  <div className="wf" style={{ width: 1280, height: 760, position: 'relative' }}>
    <div style={{ position: 'absolute', inset: 0, background:
      'repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 2px, transparent 2px 14px), var(--paper-2)' }} />
    <div style={{ position: 'absolute', left: 200, top: 80, display: 'grid', gridTemplateColumns: 'repeat(8, 56px)', gap: 4, opacity: 0.85 }}>
      {Array.from({ length: 24 }).map((_, i) => {
        const f = ['timurid','slavic',null,'kazakh',null,'ming','timurid','slavic'][i%8];
        return <HexMini key={i} faction={f} label="·" hatch={i === 9} />;
      })}
    </div>

    {/* Bottom drawer */}
    <div style={{ position: 'absolute', left: 0, right: 0, bottom: 0, height: 230, background: 'var(--paper-2)',
      borderTop: '2.5px solid var(--ink)' }}>
      <Ornament style={{ position: 'absolute', left: 0, right: 0, top: -10 }} />
      <div style={{ padding: 14, display: 'grid', gridTemplateColumns: '1fr 70px 1fr 200px', gap: 14, height: '100%' }}>
        {/* Attacker */}
        <div className="box thick" style={{ padding: 12, background: 'var(--timurid-soft)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Placeholder faction="timurid" style={{ width: 44, height: 44, fontSize: 9 }}>atk</Placeholder>
            <div>
              <div className="title-serif" style={{ fontSize: 15 }}>Tamerlane</div>
              <Tag>from Samarqand · 5 warr</Tag>
            </div>
          </div>
          <div style={{ display: 'flex', gap: 8, marginTop: 10 }}>
            <div className="die atk">6</div><div className="die atk">4</div><div className="die atk">2</div>
          </div>
        </div>

        <div style={{ display: 'grid', placeItems: 'center', textAlign: 'center' }}>
          <div>
            <div className="title-serif" style={{ fontSize: 24 }}>VS</div>
            <Ornament variant="thin" style={{ margin: '6px 0', width: 50 }} />
          </div>
        </div>

        {/* Defender */}
        <div className="box thick" style={{ padding: 12, background: 'var(--slavic-soft)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Placeholder faction="slavic" style={{ width: 44, height: 44, fontSize: 9 }}>def</Placeholder>
            <div>
              <div className="title-serif" style={{ fontSize: 15 }}>Donskoy</div>
              <Tag>defending Kashgar · 3 warr</Tag>
            </div>
          </div>
          <div style={{ display: 'flex', gap: 8, marginTop: 10 }}>
            <div className="die def">5</div><div className="die def">3</div>
          </div>
          <Tag style={{ display: 'block', marginTop: 6 }}>defender wins ties</Tag>
        </div>

        {/* Result + actions */}
        <div className="box" style={{ padding: 10 }}>
          <Tag>result</Tag>
          <div className="title-serif" style={{ fontSize: 14, marginTop: 4 }}>Defender −2 · Attacker −0</div>
          <Ornament variant="thin" style={{ margin: '8px 0' }} />
          <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
            <Btn felt="timurid" hovered style={{ padding: '6px 10px', fontSize: 13 }}>ATTACK AGAIN</Btn>
            <Btn style={{ padding: '6px 10px', fontSize: 13 }}>STOP ATTACKING</Btn>
          </div>
        </div>
      </div>
    </div>
    <Annotate x={40} y={20} w={300}>map stays visible · drawer slides up</Annotate>
  </div>
);

// ===== Tooltips (simplified: city, owner, warriors) =====
const TooltipA2 = () => (
  <div className="wf" style={{ width: 600, height: 280, position: 'relative', padding: 30 }}>
    <Tag>compact · city name / owner / warrior count</Tag>
    <div className="box thick" style={{ padding: 12, marginTop: 6, background: 'var(--kazakh-soft)', width: 260 }}>
      <div className="title-serif" style={{ fontSize: 18 }}>Astana</div>
      <Ornament variant="thin" style={{ margin: '6px 0' }} />
      <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
        <Chip faction="kazakh">Zhanibek Khan</Chip>
        <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
          <span style={{ fontSize: 13 }}>⚔</span>
          <span className="title-serif" style={{ fontSize: 18 }}>5</span>
        </span>
      </div>
    </div>
    <Annotate x={310} y={60} w={240} dir="left">hover any hex · felt bg tints to owner</Annotate>
  </div>
);

const TooltipB2 = () => (
  <div className="wf" style={{ width: 600, height: 280, position: 'relative', padding: 30 }}>
    <Tag>horizontal row · icon-based</Tag>
    <div className="box" style={{ padding: '8px 14px', marginTop: 6, background: 'var(--timurid-soft)', width: 340,
      display: 'grid', gridTemplateColumns: 'auto 1fr auto', gap: 10, alignItems: 'center' }}>
      <Placeholder faction="timurid" style={{ width: 36, height: 36, fontSize: 8 }}>·</Placeholder>
      <div>
        <div className="title-serif" style={{ fontSize: 16 }}>Samarqand</div>
        <Tag>Tamerlane · Timurid</Tag>
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
        <span style={{ fontSize: 14 }}>⚔</span>
        <span className="title-serif" style={{ fontSize: 20 }}>8</span>
      </div>
    </div>
    <Annotate x={390} y={60} w={180} dir="left">avatar + city + warriors in one row</Annotate>
  </div>
);

const TooltipC2 = () => (
  <div className="wf" style={{ width: 600, height: 280, position: 'relative', padding: 30 }}>
    <Tag>neutral / unowned tile</Tag>
    <div className="box" style={{ padding: '8px 12px', marginTop: 6, background: 'var(--paper-2)', width: 240 }}>
      <div className="title-serif" style={{ fontSize: 16 }}>Altai Mountains</div>
      <Ornament variant="thin" style={{ margin: '4px 0' }} />
      <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
        <Tag>neutral</Tag>
        <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
          <span style={{ fontSize: 13 }}>⚔</span>
          <span className="title-serif" style={{ fontSize: 16 }}>0</span>
        </span>
      </div>
    </div>
    <Annotate x={290} y={60} w={260} dir="left">neutral tiles — muted, no faction tint</Annotate>
  </div>
);

Object.assign(window, {
  CombatA2, CombatB2, CombatC2,
  TooltipA2, TooltipB2, TooltipC2,
});
