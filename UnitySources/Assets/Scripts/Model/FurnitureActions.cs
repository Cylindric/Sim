using UnityEngine;

namespace Assets.Scripts.Model
{
    public static class FurnitureActions
    {
        public static void Door_UpdateAction(Furniture furn, float deltaTime)
        {
            if (furn.furnParameters["is_opening"] >= 1f)
            {
                furn.furnParameters["openess"] += deltaTime*4;
                if (furn.furnParameters["openess"] >= 1)
                {
                    furn.furnParameters["is_opening"] = 0;
                }
            }
            else
            {
                furn.furnParameters["openess"] -= deltaTime * 4;
            }

            furn.furnParameters["openess"] = Mathf.Clamp01(furn.furnParameters["openess"]);
        }

        public static Enterability Door_IsEnterable(Furniture furn)
        {
            furn.furnParameters["is_opening"] = 1;

            if (furn.furnParameters["openess"] >= 1)
            {
                return Enterability.Yes;
            }

            return Enterability.Soon;
        }
    }
}
