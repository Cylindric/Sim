using UnityEngine;

namespace Assets.Scripts.Model
{
    public static class FurnitureActions
    {
        public static void Door_UpdateAction(Furniture furn, float deltaTime)
        {
            if (furn.GetParameter("is_opening") >= 1f)
            {
                furn.OffsetParameter("openness", deltaTime*4);

                if (furn.GetParameter("openness") >= 1f)
                {
                    furn.SetParameter("is_opening", 0);
                }
            }
            else
            {
                furn.OffsetParameter("openness", deltaTime * -4);
            }

            furn.SetParameter("openness", Mathf.Clamp01(furn.GetParameter("openness")));
            furn.cbOnChanged(furn);
        }

        public static Enterability Door_IsEnterable(Furniture furn)
        {
            furn.SetParameter("is_opening", 1);

            if (furn.GetParameter("openness") >= 1)
            {
                return Enterability.Yes;
            }

            return Enterability.Soon;
        }
    }
}
