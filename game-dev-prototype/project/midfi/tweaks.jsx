/* ====== Mid-fi Tweaks ====== */

const MfTweaks = () => {
  const [tw, setTweak] = useTweaks(window.MF_TWEAKS_DEFAULTS);

  React.useEffect(() => {
    const root = document.getElementById('canvas-root');
    if (!root) return;
    root.classList.toggle('no-orn', !tw.ornaments);
  }, [tw]);

  return (
    <TweaksPanel title="Tweaks">
      <TweakSection title="Display">
        <TweakToggle label="Cultural ornaments" value={tw.ornaments}
          onChange={v => setTweak('ornaments', v)} />
        <TweakToggle label="Show tooltip" value={tw.tooltipVisible}
          onChange={v => setTweak('tooltipVisible', v)} />
      </TweakSection>
      <TweakSection title="Active faction tint">
        <TweakRadio value={tw.activeFaction}
          onChange={v => setTweak('activeFaction', v)}
          options={[
            { value: 'kazakh', label: 'Kazakh' },
            { value: 'slavic', label: 'Slavic' },
            { value: 'timurid', label: 'Timurid' },
            { value: 'ming', label: 'Ming' },
          ]} />
      </TweakSection>
      <TweakSection title="Phase">
        <TweakRadio value={tw.phase}
          onChange={v => setTweak('phase', v)}
          options={[
            { value: 'setup', label: 'Setup' },
            { value: 'reinforce', label: 'Reinf.' },
            { value: 'attack', label: 'Attack' },
            { value: 'fortify', label: 'Fortify' },
          ]} />
      </TweakSection>
    </TweaksPanel>
  );
};

window.MfTweaks = MfTweaks;
