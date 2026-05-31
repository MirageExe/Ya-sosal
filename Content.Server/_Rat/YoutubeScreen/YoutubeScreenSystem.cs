using Content.Shared._Rat.YoutubeScreen;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;

namespace Content.Server._Rat.YoutubeScreen;

public sealed class YoutubeScreenSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<YoutubeScreenComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<YoutubeScreenComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<YoutubeScreenComponent, ExaminedEvent>(OnExamined);
    }

    private void OnActivate(Entity<YoutubeScreenComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        if (!TryTogglePlayback(ent))
            return;

        args.Handled = true;
    }

    private void OnInteractHand(Entity<YoutubeScreenComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryTogglePlayback(ent))
            return;

        args.Handled = true;
    }

    private bool TryTogglePlayback(Entity<YoutubeScreenComponent> ent)
    {
        if (string.IsNullOrEmpty(ent.Comp.VideoId))
            return false;

        ent.Comp.Playing = !ent.Comp.Playing;
        Dirty(ent);
        return true;
    }

    private void OnExamined(Entity<YoutubeScreenComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (string.IsNullOrEmpty(ent.Comp.VideoId))
        {
            args.PushMarkup(Loc.GetString("youtube-screen-examine-empty"));
            return;
        }

        args.PushMarkup(Loc.GetString("youtube-screen-examine-video",
            ("id", ent.Comp.VideoId),
            ("state", ent.Comp.Playing
                ? Loc.GetString("youtube-screen-examine-playing")
                : Loc.GetString("youtube-screen-examine-paused"))));
    }

    public bool TrySetVideo(Entity<YoutubeScreenComponent> ent, string? videoIdOrUrl, bool? playing = null)
    {
        var id = videoIdOrUrl == null ? null : YoutubeUrl.ParseVideoId(videoIdOrUrl);
        if (videoIdOrUrl != null && id == null)
            return false;

        ent.Comp.VideoId = id ?? string.Empty;
        if (playing != null)
            ent.Comp.Playing = playing.Value;
        else if (!string.IsNullOrEmpty(ent.Comp.VideoId))
            ent.Comp.Playing = true;

        if (string.IsNullOrEmpty(ent.Comp.VideoId))
            ent.Comp.Playing = false;

        Dirty(ent);
        return true;
    }
}
