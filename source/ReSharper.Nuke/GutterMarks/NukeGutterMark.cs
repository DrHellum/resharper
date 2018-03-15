﻿// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/ide-extensions/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.Util;
using ReSharper.Nuke.GutterMarks;
using ReSharper.Nuke.Highlightings;
using ReSharper.Nuke.Resources;

[assembly:
    RegisterHighlighter(NukeHighlitingAttributeIds.NukeGutterIconAttribute,
        EffectType = EffectType.GUTTER_MARK,
        GutterMarkType = typeof(NukeGutterMark),
        Layer = HighlighterLayer.SYNTAX + 1)]

namespace ReSharper.Nuke.GutterMarks
{
    public class NukeGutterMark : IconGutterMark
    {
        public NukeGutterMark()
            : base(LogoThemedIcons.NukeLogo.Id)
        {
        }

        public override IAnchor Anchor => BulbMenuAnchors.FirstClassContextItems;

        public override IEnumerable<BulbMenuItem> GetBulbMenuItems(IHighlighter highlighter)
        {
            var solution = Shell.Instance.GetComponent<SolutionsManager>().Solution;
            if (solution == null)
                return EmptyList<BulbMenuItem>.InstanceList;

            var textControlManager = solution.GetComponent<ITextControlManager>();
            var textControl = textControlManager.FocusedTextControl.Value;

            var daemon = solution.GetComponent<IDaemon>();
            var highlighting = daemon.GetHighlighting(highlighter) as NukeMarkOnGutter;
            if (highlighting != null)
            {
                using (CompilationContextCookie.GetExplicitUniversalContextIfNotSet())
                    return highlighting.GetBulbMenuItems(solution, textControl, (InvisibleAnchor) Anchor);
            }

            return EmptyList<BulbMenuItem>.InstanceList;
        }
    }
}