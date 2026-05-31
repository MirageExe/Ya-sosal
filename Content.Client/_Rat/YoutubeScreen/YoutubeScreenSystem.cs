using System.Collections.Generic;
using System.Numerics;
using Content.Shared._Rat.YoutubeScreen;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.WebView;
using Robust.Shared.GameStates;
using Robust.Shared.IoC;

namespace Content.Client._Rat.YoutubeScreen;

public sealed class YoutubeScreenSystem : EntitySystem
{
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    private readonly Dictionary<EntityUid, WebViewControl> _views = new();
    private LayoutContainer? _layer;
    private bool _webViewAvailable;

    public override void Initialize()
    {
        base.Initialize();

        _webViewAvailable = IoCManager.Instance?.TryResolveType<IWebViewManager>(out _) == true;
        if (!_webViewAvailable)
        {
            Log.Warning("Robust.Client.WebView is not loaded — YouTube screens will not render.");
            return;
        }

        _layer = new LayoutContainer
        {
            Name = "YoutubeScreenLayer",
            MouseFilter = Control.MouseFilterMode.Ignore,
        };
        LayoutContainer.SetAnchorLeft(_layer, 0);
        LayoutContainer.SetAnchorTop(_layer, 0);
        LayoutContainer.SetAnchorRight(_layer, 1);
        LayoutContainer.SetAnchorBottom(_layer, 1);
        _ui.WindowRoot.AddChild(_layer);

        SubscribeLocalEvent<YoutubeScreenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<YoutubeScreenComponent, AfterAutoHandleStateEvent>(OnState);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        foreach (var view in _views.Values)
            view.Dispose();

        _views.Clear();
        _layer?.Dispose();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (!_webViewAvailable || _layer == null)
            return;

        var eye = _eye.CurrentEye;
        var eyePos = eye.Position;
        var active = new HashSet<EntityUid>();

        var query = EntityQueryEnumerator<YoutubeScreenComponent, TransformComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform, out _))
        {
            if (!comp.Playing || string.IsNullOrEmpty(comp.VideoId))
            {
                Hide(uid);
                continue;
            }

            if (xform.MapID != eyePos.MapId)
            {
                Hide(uid);
                continue;
            }

            var worldPos = _xform.GetWorldPosition(xform);
            if ((worldPos - eyePos.Position).Length() > comp.MaxDistance)
            {
                Hide(uid);
                continue;
            }

            var screenPos = _eye.WorldToScreen(worldPos);
            if (!IsOnScreen(screenPos))
            {
                Hide(uid);
                continue;
            }

            active.Add(uid);
            var view = EnsureView(uid, comp);
            view.Visible = true;

            var size = new Vector2(comp.DisplaySize.X, comp.DisplaySize.Y);
            view.SetSize = size;
            LayoutContainer.SetMarginLeft(view, screenPos.X - size.X / 2f);
            LayoutContainer.SetMarginTop(view, screenPos.Y - size.Y / 2f);
            LayoutContainer.SetMarginRight(view, screenPos.X + size.X / 2f);
            LayoutContainer.SetMarginBottom(view, screenPos.Y + size.Y / 2f);
        }

        foreach (var (uid, view) in _views)
        {
            if (!active.Contains(uid))
                view.Visible = false;
        }
    }

    private void OnState(Entity<YoutubeScreenComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!_webViewAvailable)
            return;

        if (!ent.Comp.Playing || string.IsNullOrEmpty(ent.Comp.VideoId))
        {
            Hide(ent);
            return;
        }

        if (_views.TryGetValue(ent, out var view))
            view.Url = YoutubeEmbed.ToResPageUrl(ent.Comp.VideoId, ent.Comp.Playing);
    }

    private void OnShutdown(Entity<YoutubeScreenComponent> ent, ref ComponentShutdown args)
    {
        Hide(ent);
    }

    private WebViewControl EnsureView(EntityUid uid, YoutubeScreenComponent comp)
    {
        if (_views.TryGetValue(uid, out var existing))
        {
            existing.Url = YoutubeEmbed.ToResPageUrl(comp.VideoId, comp.Playing);
            return existing;
        }

        var view = new WebViewControl
        {
            Name = $"YoutubeScreen-{uid}",
            AlwaysActive = true,
            MouseFilter = Control.MouseFilterMode.Ignore,
        };
        LayoutContainer.SetAnchorLeft(view, 0);
        LayoutContainer.SetAnchorTop(view, 0);
        view.Url = YoutubeEmbed.ToResPageUrl(comp.VideoId, comp.Playing);
        _layer!.AddChild(view);
        _views[uid] = view;
        return view;
    }

    private void Hide(EntityUid uid)
    {
        if (_views.TryGetValue(uid, out var view))
            view.Visible = false;
    }

    private static bool IsOnScreen(Vector2 screenPos)
    {
        return screenPos.X > -5000 && screenPos.Y > -5000;
    }
}
