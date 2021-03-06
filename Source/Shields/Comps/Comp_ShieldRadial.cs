﻿using System.Collections.Generic;
using FrontierDevelopments.General;
using FrontierDevelopments.Shields.CompProperties;
using FrontierDevelopments.Shields.Windows;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Comps
{
    public class Comp_ShieldRadial : ThingComp
    {
        private int _cellCount;
        private int _fieldRadius;
        private bool _renderField = true;
        
        public CompProperties_ShieldRadial Props => 
            (CompProperties_ShieldRadial)props;

        public int Radius
        {
            get => _fieldRadius;
            set
            {
                if (value < 0)
                {
                    _fieldRadius = 0;
                    return;
                }
                if (value > Props.maxRadius)
                {
                    _fieldRadius = Props.maxRadius;
                    return;
                }
                _fieldRadius = value;
                _cellCount = GenRadial.NumCellsInRadius(_fieldRadius);
            }
        }

        public override void Initialize(Verse.CompProperties props)
        {
            base.Initialize(props);
            Radius = Props.maxRadius;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var current in base.CompGetGizmosExtra())
                yield return current;
            
            yield return new Command_Toggle
            {
                icon = Resources.UiBlank,
                defaultDesc = "fd.shield.render_field.description".Translate(),
                defaultLabel = "fd.shield.render_field.label".Translate(),
                isActive = () => _renderField,
                toggleAction = () => _renderField = !_renderField
            };

            if (parent.Faction == Faction.OfPlayer)
            {
                if (Props.minRadius != Props.maxRadius)
                {
                    yield return new Command_Action
                    {
                        icon = Resources.UiSetRadius,
                        defaultDesc = "radius.description".Translate(),
                        defaultLabel = "radius.label".Translate(),
                        activateSound = SoundDef.Named("Click"),
                        action = () => Find.WindowStack.Add(new Popup_IntSlider("radius.label".Translate(), Props.minRadius, Props.maxRadius, () => Radius, size =>  Radius = size))
                    };
                }
            }
        }

        public bool Collision(Vector2 vector)
        {
            return Vector2.Distance(Common.ToVector2(parent.Position), vector) < _fieldRadius + 0.5f;
        }

        public Vector2? Collision(Ray2D ray, float limit)
        {
            var point = ray.GetPoint(limit);
            if (Collision(point))
            {
                return ray.origin;
            }
            return null;
        }

        public int ProtectedCellCount()
        {
            return _cellCount;
        }
        
        public void Draw(CellRect cameraRect)
        {
            if (!_renderField) return;
            if (!cameraRect.Overlaps(CellRect.CenteredOn(parent.Position, Radius))) return;
            var position = Common.ToVector3(parent.Position);
            position.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
            var scalingFactor = (float)(_fieldRadius * 2.2);
            var scaling = new Vector3(scalingFactor, 1f, scalingFactor);
            var matrix = new Matrix4x4();
            matrix.SetTRS(position, Quaternion.AngleAxis(0, Vector3.up), scaling);
            Graphics.DrawMesh(MeshPool.plane10, matrix, Resources.ShieldMat, 0);
        }
        
        public override void PostDrawExtraSelectionOverlays()
        {
            GenDraw.DrawRadiusRing(parent.Position, _fieldRadius);
        }
        
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _fieldRadius, "radius");
            Scribe_Values.Look(ref _renderField, "renderField", true);
        }
    }
}