/* ====== Hi-fi: App shell — state machine, scaling, transitions ====== */

const SCREENS = { MENU: 'menu', HOTSEAT: 'hotseat', HUD: 'hud', GAMEOVER: 'gameover' };

const HfApp = () => {
  const [screen, setScreen] = React.useState(SCREENS.MENU);
  const [combat, setCombat] = React.useState(null); // { attacker, defender } or null
  const [gameState, setGameState] = React.useState({
    activePlayer: 'kazakh',
    phase: 'reinforce',
    showTooltip: false,
    tooltipData: null,
  });

  // Scaling
  const [scale, setScale] = React.useState(1);
  React.useEffect(() => {
    const resize = () => {
      const sx = window.innerWidth / 1920;
      const sy = window.innerHeight / 1080;
      setScale(Math.min(sx, sy));
    };
    resize();
    window.addEventListener('resize', resize);
    return () => window.removeEventListener('resize', resize);
  }, []);

  // Screen navigation
  const go = (s) => { setCombat(null); setScreen(s); };

  return (
    <div className="hifi-stage">
      <div className="hifi-canvas" style={{ transform: `scale(${scale})` }}>
        {/* Current screen */}
        <div key={screen} className="screen-enter" style={{ position: 'absolute', inset: 0 }}>
          {screen === SCREENS.MENU && (
            <HfMenu
              onStart={() => go(SCREENS.HOTSEAT)}
              onOptions={() => {}}
            />
          )}
          {screen === SCREENS.HOTSEAT && (
            <HfHotseat
              onStart={(p1, p2) => {
                if (p1 && p2) {
                  setGameState(s => ({ ...s, activePlayer: p1, phase: 'reinforce' }));
                  go(SCREENS.HUD);
                } else {
                  go(SCREENS.MENU);
                }
              }}
            />
          )}
          {screen === SCREENS.HUD && (
            <HfHud
              gameState={gameState}
              setGameState={setGameState}
              onCombat={(defender) => setCombat({ attacker: gameState.activePlayer, defender })}
              onEndTurn={() => {
                const factions = ['kazakh', 'timurid', 'slavic', 'ming'];
                const next = factions[(factions.indexOf(gameState.activePlayer) + 1) % 4];
                setGameState(s => ({ ...s, activePlayer: next, phase: 'reinforce' }));
              }}
            />
          )}
          {screen === SCREENS.GAMEOVER && (
            <HfGameOver
              winner="kazakh"
              onRematch={() => {
                setGameState({ activePlayer: 'kazakh', phase: 'reinforce', showTooltip: false, tooltipData: null });
                go(SCREENS.HUD);
              }}
              onMenu={() => go(SCREENS.MENU)}
            />
          )}
        </div>

        {/* Combat overlay on top of HUD */}
        {combat && screen === SCREENS.HUD && (
          <HfCombat
            attacker={combat.attacker}
            defender={combat.defender}
            onClose={() => setCombat(null)}
            onAttackAgain={() => {}}
          />
        )}

        {/* Navigation debug strip — bottom right */}
        <div style={{
          position: 'absolute', bottom: 12, right: 12, zIndex: 100,
          display: 'flex', gap: 6,
        }}>
          {[
            { s: SCREENS.MENU, label: 'Menu' },
            { s: SCREENS.HOTSEAT, label: 'Select' },
            { s: SCREENS.HUD, label: 'HUD' },
            { s: 'combat', label: 'Combat' },
            { s: SCREENS.GAMEOVER, label: 'Game Over' },
          ].map(n => (
            <div key={n.s}
              className="pointer"
              onClick={() => {
                if (n.s === 'combat') {
                  if (screen !== SCREENS.HUD) go(SCREENS.HUD);
                  setTimeout(() => setCombat({ attacker: gameState.activePlayer, defender: 'slavic' }), 100);
                } else {
                  go(n.s);
                }
              }}
              style={{
                padding: '4px 10px', borderRadius: 3,
                background: screen === n.s ? 'rgba(200,168,78,0.3)' : 'rgba(0,0,0,0.5)',
                border: `1px solid ${screen === n.s ? 'var(--gold)' : 'rgba(200,168,78,0.2)'}`,
                fontFamily: 'var(--font-label)', fontSize: 9, color: 'var(--gold)',
                textTransform: 'uppercase', letterSpacing: '0.06em',
              }}>
              {n.label}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

Object.assign(window, { HfApp });
