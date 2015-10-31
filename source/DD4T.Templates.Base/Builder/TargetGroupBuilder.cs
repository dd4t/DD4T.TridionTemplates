using System.Collections.Generic;
using Tridion.ContentManager.Templating;
using Dynamic = DD4T.ContentModel;
using Tcm = Tridion.ContentManager.AudienceManagement;

namespace DD4T.Templates.Base.Builder
{
    /// <summary>
    ///     TargetGroupBuilder is responsible for mapping a Tridion Target Group and associated conditions to a DD4T Content
    ///     Model Target Group and conditions
    /// </summary>
    public class TargetGroupBuilder
    {
        private static readonly TemplatingLogger Log = TemplatingLogger.GetLogger(typeof (TargetGroupBuilder));

        /// <summary>
        ///     Build a DD4T Target group from a AM Target Group
        /// </summary>
        public static Dynamic.TargetGroup BuildTargetGroup(Tcm.TargetGroup targetGroup, BuildManager buildManager)
        {
            var tg = new Dynamic.TargetGroup
            {
                Title = targetGroup.Title,
                Id = targetGroup.Id,
                Description = targetGroup.Description,
                OwningPublication = PublicationBuilder.BuildPublication(targetGroup.OwningRepository),
                Publication = PublicationBuilder.BuildPublication(targetGroup.ContextRepository),
                PublicationId = targetGroup.ContextRepository.Id,
                Conditions = MapConditions(targetGroup.Conditions, buildManager)
            };

            return tg;
        }

        /// <summary>
        ///     Map the conditions from a Component Presentaton to DD4T conditions
        /// </summary>
        public static List<Dynamic.Condition> MapTargetGroupConditions(
            IList<Tcm.TargetGroupCondition> componentPresentationConditions, BuildManager buildManager)
        {
            var mappedConditions = new List<Dynamic.Condition>();
            foreach (var componentPresentationCondition in componentPresentationConditions)
            {
                mappedConditions.AddRange(MapConditions(componentPresentationCondition.TargetGroup.Conditions,
                    buildManager));
            }

            return mappedConditions;
        }

        private static List<Dynamic.Condition> MapConditions(IList<Tcm.Condition> conditions, BuildManager buildManager)
        {
            var mappedConditions = new List<Dynamic.Condition>();
            foreach (var condition in conditions)
            {
                if (condition is Tcm.TrackingKeyCondition)
                {
                    mappedConditions.Add(MapTrackingKeyCondition((Tcm.TrackingKeyCondition) condition, buildManager));
                }
                else if (condition is Tcm.TargetGroupCondition)
                {
                    mappedConditions.Add(MapTargetGroupCondition((Tcm.TargetGroupCondition) condition, buildManager));
                }
                else if (condition is Tcm.CustomerCharacteristicCondition)
                {
                    mappedConditions.Add(
                        MapCustomerCharacteristicCondition((Tcm.CustomerCharacteristicCondition) condition));
                }
                else
                {
                    Log.Warning("Condition of type: " + condition.GetType().FullName +
                                " was not supported by the mapping code.");
                }
            }
            return mappedConditions;
        }

        private static Dynamic.CustomerCharacteristicCondition MapCustomerCharacteristicCondition(
            Tcm.CustomerCharacteristicCondition condition)
        {
            var newCondition = new Dynamic.CustomerCharacteristicCondition
            {
                Value = condition.Value,
                Operator = (Dynamic.ConditionOperator) condition.Operator,
                Name = condition.Name,
                Negate = condition.Negate
            };
            return newCondition;
        }

        private static Dynamic.TargetGroupCondition MapTargetGroupCondition(
            Tcm.TargetGroupCondition targetGroupCondition, BuildManager buildManager)
        {
            var newCondition = new Dynamic.TargetGroupCondition
            {
                TargetGroup = BuildTargetGroup(targetGroupCondition.TargetGroup, buildManager),
                Negate = targetGroupCondition.Negate
            };
            return newCondition;
        }

        private static Dynamic.KeywordCondition MapTrackingKeyCondition(Tcm.TrackingKeyCondition trackingKeyCondition,
            BuildManager buildManager)
        {
            var newCondition = new Dynamic.KeywordCondition
            {
                Keyword = KeywordBuilder.BuildKeyword(trackingKeyCondition.Keyword, buildManager),
                Operator = (Dynamic.NumericalConditionOperator) trackingKeyCondition.Operator,
                Negate = true,
                Value = trackingKeyCondition.Value
            };
            return newCondition;
        }
    }
}