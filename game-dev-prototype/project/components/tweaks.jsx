/* ============ Tweaks panel ============ */

const RonTweaks = () => {
  const [tw, setTweak] = useTweaks(window.RON_TWEAKS_DEFAULTS);

  // apply globally via root class
  React.useEffect(() => {
    const root = document.getElementById('canvas-root');
    if (!root) return;
    root.classList.toggle('no-orn', !tw.ornaments);
    root.dataset.tooltipVisible = tw.tooltipVisible ? '1' : '0';
    root.dataset.activeFaction = tw.activeFaction;
    root.dataset.phase = tw.phase;
  }, [tw]);

  return (
    <TweaksPanel title="Tweaks">
      <TweakSection title="Display">
        <TweakToggle label="Cultural ornament strips" value={tw.ornaments}
          onChange={v => setTweak('ornaments', v)} />
        <TweakToggle label="Show tile tooltips" value={tw.tooltipVisible}
          onChange={v => setTweak('tooltipVisible', v)} />
      </TweakSection>
      <TweakSection title="Active faction (HUD tint)">
        <TweakRadio value={tw.activeFaction}
          onChange={v => setTweak('activeFaction', v)}
          options={[
            { value: 'kazakh', label: 'Kazakh' },
            { value: 'slavic', label: 'Slavic' },
            { value: 'timurid', label: 'Timurid' },
            { value: 'ming', label: 'Ming' },
          ]} />
      </TweakSection>
      <TweakSection title="Game phase">
        <TweakRadio value={tw.phase}
          onChange={v => setTweak('phase', v)}
          options={[
            { value: 'reinforce', label: 'Reinforce' },
            { value: 'action', label: 'Action' },
            { value: 'migration', label: 'Migr.' },
          ]} />
      </TweakSection>
    </TweaksPanel>
  );
};

window.RonTweaks = RonTweaks;
