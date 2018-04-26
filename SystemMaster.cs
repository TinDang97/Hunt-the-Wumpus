using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wumpus
{

    public static class Events
    {
        public const String CATCH = "CATCH";
        public const String GO = "GO";
        public const String SHOT = "SHOT";

        public const String TOP = "TOP";
        public const String BOT = "BOT";
        public const String LEFT = "LEFT";
        public const String RIGHT = "RIGHT";

    }

    public static class Items
    {
        public const String GOLD = "GOLD";
        public const String MONSTER = "MONSTER";
        public const String HOLE = "HOLE";
        public const String START = "START";
    }

    public class Property : ICloneable
    {
        public Boolean Stench = false;
        public Boolean Breeze = false;

        public static String Convert2Str(Property x)
        {
            String result = "";

            if (x.Breeze)
                result += "Breeze\n";

            if (x.Stench)
                result += "Stench\n";

            return result;
        }

        public Object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class Squares
    {
        public String Item;
        public Property Property;

        public Squares TOP, BOT, LEFT, RIGHT;

        public Squares()
        {
            Item = String.Empty;
            Property = new Property();

            TOP = null;
            BOT = null;
            LEFT = null;
            RIGHT = null;
        }

        public static Squares GetSquares(Squares x)
        {
            Squares nItem = new Squares();

            nItem.Item = x.Item;
            nItem.Property = (Property) x.Property.Clone();

            return nItem;
        }
    }

    class SystemMaster
    {
        List<List<Squares>> MapGame;
        int Size = 0;
        
        Random random = new Random();

        Squares StartSquare;
        Squares CurrSquare;

        Boolean caughtGOLD = false;

        String direction = "";

        Action<Point, Object , Boolean> funcShow;
        Action<Object, Boolean> funcAgent;

        public SystemMaster(int Size) 
        {
            this.Size = Size;
            MapGame = new List<List<Squares>>(Size);
            for(int x = 0; x < Size; x++)
            {
                List<Squares> hor = new List<Squares>(Size);
                for (int y = 0; y < Size; y++)
                    hor.Add(new Squares());
                MapGame.Add(hor);
            }
        }

        public Boolean Init(int NumOfMonters, int NumOfHoles, Action<Point, Object ,Boolean> funcShow)
        {
            try
            {
                this.funcShow = funcShow;

                CreateItems(NumOfMonters, NumOfHoles);
                CreateProperty();
                RequestStartSquare();
                ConnectMap();

                direction = Events.TOP;
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public void InitAgent(Action<Object, Boolean> funcAgent)
        {
            this.funcAgent = funcAgent;
        }

        private void ConnectMap()
        {
            int X, Y;
            for (int n = 0; n < Size * Size; n++)
            {
                X = n / Size;
                Y = n % Size;

                Squares current = MapGame[X][Y];

                if (Y - 1 >= 0)
                    current.TOP = MapGame[X][Y - 1];

                if (Y + 1 < Size)
                    current.BOT = MapGame[X][Y + 1];

                if (X - 1 >= 0)
                    current.LEFT = MapGame[X - 1][Y];

                if (X + 1 < Size)
                    current.RIGHT = MapGame[X + 1][Y];
            }
        }

        public List<List<Squares>> getMap()
        {
            return MapGame;
        }

        public Squares GetStartSquares()
        {
            return Squares.GetSquares(StartSquare);
        }

        public String GetDirection()
        {
            return direction;
        }
        public void RequestStartSquare()
        {
            List<int> Positions = new List<int>(Size * Size);

            for (int n = 0; n < Size * Size; n++)
            {
                Positions.Add(n);
            }

            int Position, X, Y;

            do
            {
                Position = getRandom(Positions.Count);

                X = Positions[Position] / Size;
                Y = Positions[Position] % Size;
            }
            while (!ConditionOfStart(X, Y));

            StartSquare = MapGame[X][Y];
            CurrSquare = StartSquare;
            CurrSquare.Item = Items.START;
        }

        private Boolean ConditionOfStart(int X, int Y)
        {
            Squares item = MapGame[X][Y];

            if (item.Item.Equals(Items.MONSTER) || item.Item.Equals(Items.HOLE) 
                || item.Property.Breeze || item.Property.Stench)
                return false;

            return true;
        }

        private void CreateProperty()
        {
            int X = 0, Y = 0;
            foreach(List<Squares> hor in MapGame)
            {
                Y = 0;
                foreach (Squares square in hor)
                {
                    if (square.Item.Equals(Items.MONSTER))
                    {
                        for (int dx = -1; dx < 2; dx += 2)
                            if (dx + X < Size && dx + X >= 0)
                                MapGame[dx + X][Y].Property.Stench = true;

                        for (int dy = -1; dy < 2; dy += 2)
                            if (dy + Y < Size && dy + Y >= 0)
                                MapGame[X][dy + Y].Property.Stench = true;
                    }


                    else if (square.Item.Equals(Items.HOLE))
                    {
                        for (int dx = -1; dx < 2; dx += 2)
                            if (dx + X < Size && dx + X >= 0)
                                MapGame[dx + X][Y].Property.Breeze = true;

                        for (int dy = -1; dy < 2; dy += 2)
                            if (dy + Y < Size && dy + Y >= 0)
                                MapGame[X][dy + Y].Property.Breeze = true;
                    }
                    
                    Y++;
                }
                X++;
            }
        }

        private void CreateItems(int NumOfMonters, int NumOfHoles)
        {

            List<int> Positions = new List<int>(Size * Size);

            for (int n = 0; n < Size * Size; n++)
            {
                Positions.Add(n);
            }

            for (int n = 0; n < NumOfMonters; n++)
            {
                int X, Y;
                int Position;

                do
                {
                    Position = getRandom(Positions.Count);

                    X = Positions[Position] / Size;
                    Y = Positions[Position] % Size;
                }
                while (CheckOutOfRules(X, Y));

                MapGame[X][Y].Item = Items.MONSTER;
                Positions.Remove(Position);
            }

            for (int n = 0; n < NumOfHoles; n++)
            {
                int X, Y;
                int Position;

                do
                {
                    Position = getRandom(Positions.Count);

                    X = Positions[Position] / Size;
                    Y = Positions[Position] % Size;
                }
                while (CheckOutOfRules(X, Y));

                MapGame[X][Y].Item = Items.HOLE;
                Positions.RemoveAt(Position);
            }

            { 
                int X, Y;
                int Position;

                Position = getRandom(Positions.Count);

                X = Positions[Position] / Size;
                Y = Positions[Position] % Size;

                MapGame[X][Y].Item = Items.GOLD;
                Console.WriteLine("GOLD:  " + X + " - " + Y);
                Positions.Clear();
            }

        }


        private Boolean CheckOutOfRules(int dx, int dy) //Monsters and Holes can't occupy full line. RULE
        {
            Boolean OutOfRules = true;

            for(int x = 0; x < Size; x++)
                if (MapGame[x][dy].Item.Equals(String.Empty) || MapGame[x][dy].Item.Equals(Items.GOLD))
                    OutOfRules = false;

            if (OutOfRules)
                return true;

            for (int y = 0; y < Size; y++)
                if (MapGame[dx][y].Item.Equals(String.Empty) || MapGame[dx][y].Item.Equals(Items.GOLD))
                    OutOfRules = false;

            return OutOfRules;
        }

        private int getRandom(int range) // from 0 to range-1
        {
            return random.Next(range);
        }

        public static Squares GetSquaresByDirection(Squares x, String direction)
        {
            if (direction.Equals(Events.TOP))
                return x.TOP;

            if (direction.Equals(Events.BOT))
                return x.BOT;

            if (direction.Equals(Events.LEFT))
                return x.LEFT;

            if (direction.Equals(Events.RIGHT))
                return x.RIGHT;

            return null;
        }

        public Point findLocation()
        {
            int X = 0, Y = 0;

            foreach(List<Squares> hor in MapGame)
            {
                Y = 0;
                foreach(Squares location in hor)
                {
                    if (location.Equals(CurrSquare))
                        return new Point(X, Y);

                    Y++;
                }
                X++;
            }

            return new Point(-1, -1);
        }

        public void ProcessEvent(String events)
        {
            Thread.Sleep(100);

            Console.WriteLine("Wumpus Run! " + events);
            if (events.Equals(Events.CATCH))
            {
                if (CurrSquare.Item.Equals(Items.GOLD))
                {
                    caughtGOLD = true;
                    CurrSquare.Item = "";

                    funcShow.Invoke(findLocation(), true, false);
                    funcAgent.Invoke(true, false);
                    return;
                }

                funcShow.Invoke(findLocation(), false, false);
                funcAgent.Invoke(false, false);
                return;
            }

            if (events.Equals(Events.GO))
            {
                Squares item = GetSquaresByDirection(CurrSquare, direction);

                if (item == null)
                {
                    funcShow.Invoke(findLocation(), null, false);
                    funcAgent.Invoke(null, false);
                    Console.WriteLine("System:null");
                    return;
                }

                if (item.Equals(StartSquare) && caughtGOLD)
                {
                    funcShow.Invoke(findLocation(), Squares.GetSquares(item), true);
                    funcAgent.Invoke(Squares.GetSquares(item), true);
                    return;
                }

                if (item.Item.Equals(Items.MONSTER) || item.Item.Equals(Items.HOLE))
                {
                    funcShow.Invoke(findLocation(), null, true);
                    funcAgent.Invoke(Squares.GetSquares(item), true);
                    return;
                }

                CurrSquare = item;

                funcShow.Invoke(findLocation(), Squares.GetSquares(CurrSquare), false);
                funcAgent.Invoke(Squares.GetSquares(CurrSquare), false);
                return;
            }

            if (events.Equals(Events.SHOT))
            {
                Boolean shotttt = ShotMonster(CurrSquare);
                Console.WriteLine("Shottttttt: " + shotttt);
                funcShow.Invoke(findLocation(), shotttt, false);
                funcAgent.Invoke(shotttt, false);
                return;
            }

            Boolean IsEvent = false;

            if (events.Equals(Events.TOP))
            {
                direction = Events.TOP;
                IsEvent = true;
            }

            if (events.Equals(Events.BOT))
            {
                direction = Events.BOT;
                IsEvent = true;
            }

            if (events.Equals(Events.LEFT))
            {
                IsEvent = true;
                direction = Events.LEFT;
            }                

            if (events.Equals(Events.RIGHT))
            {
                IsEvent = true;
                direction = Events.RIGHT;
            }

            if (!IsEvent)
            {
                funcShow.Invoke(findLocation(), IsEvent, true);
                return;
            }

            funcShow.Invoke(findLocation(), IsEvent, false);
            funcAgent.Invoke(IsEvent, false);
            return;
        }

        private Boolean ShotMonster(Squares x)
        {
            Squares item = GetSquaresByDirection(x, direction);

            if (item == null)
                return false;

            if (item.Item.Equals(Items.MONSTER)){
                item.Item = String.Empty;
                return true;
            }
            
            return ShotMonster(item);
        }
    }
}
