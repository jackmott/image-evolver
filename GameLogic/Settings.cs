using Microsoft.Xna.Framework.Graphics;

namespace GameLogic
{
    public static class Settings
    {
        public const int MIN_GEN_SIZE = 1;
        public const int MAX_GEN_SIZE = 5;
        public const int POP_SIZE_COLUMNS = 5;
        //Chance of a mutation happening on a pic
        public const int MUTATE_CHANCE = 2; // 1 in X
        //Chance of mutation being leaf vs node
        public const int MUTATE_LEAF_CHANCE = 5; // 1 in X
        //Chance of breeding swapping pic types
        public const int CROSSOVER_ROOT_CHANCE = 4; // 1 in X
                

        public const int PREVIEW_VIDEO_WIDTH = 320;
        public const int PREVIEW_VIDEO_HEIGHT = 200;

        //note min gradients cannot be less than 2
        public const int MIN_GRADIENTS = 2;
        public const int MAX_GRADIENTS = 10;
        public const float HORIZONTAL_SPACING = .01f;
        public const float VERTICAL_SPACING = .01f;

        public const int FPS = 30;
        public const int VIDEO_LENGTH = 5; //seconds

        public static Texture2D injectTexture;
        public static Texture2D equationTexture;
        public static Texture2D selectedTexture;
        public static Texture2D saveEquationTexture;
        public static Texture2D cancelEditTexture;
        public static Texture2D panelTexture;

        public static SpriteFont equationFont;
    }
}
