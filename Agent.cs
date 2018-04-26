using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Wumpus;
using System.Threading;

namespace Wumpus
{
    public class SquareOfAgent
    {
        public Point Location = new Point();

        public Boolean IsVisited = false;
        private Boolean IsSafe = false;

        public Property Property = new Property();
        public String Item = "";

        public int IsMonsterTime = 0;
        public int IsHoleTime = 0;

        public SquareOfAgent Top = null, Bot = null, Left = null, Right = null;

        public SquareOfAgent()
        {
            IsSafe = false;

            Top = null;
            Bot = null;
            Left = null;
            Right = null;
        }

        public void SetIsSafe(Boolean IsSafe)
        {
            this.IsSafe = IsSafe;
        }

        public Boolean GetIsSafe()
        {
            return IsSafe;
        }

        public SquareOfAgent(Property property, String Item)
        {
            IsSafe = false;
            Top = null;
            Bot = null;
            Left = null;
            Right = null;

            Property = property;
            this.Item = Item;
        }

        public void TickIsHole() {
            if (IsSafe || IsVisited || IsHoleTime == int.MaxValue)
                return;

            IsHoleTime++;

            if (IsHoleTime > 2)
                IsHoleTime = int.MaxValue;
        }
    }
    
    class Agent
    {
        SquareOfAgent Start;
        SquareOfAgent Curr;

        SquareOfAgent SquareNonVaS = null;
        SquareOfAgent SquareNonVaSBetter = null;

        List<SquareOfAgent> SquareIsMonster = null;
        
        Boolean haveGold = false;
        Boolean MonsterIsDead = false;
        Boolean Shoted = false;
        Boolean _running = true;

        String direction = "";
        String EventCMD = "";

        Action<String> funcSystem;
        Action<String, SquareOfAgent, SquareOfAgent> funcShow;

        private ManualResetEvent _manualResetEvent = new ManualResetEvent(true);

        public Agent(Squares StartSquare, String direction, Action<String> funcSystem, Action<String, SquareOfAgent, SquareOfAgent> funcShow)
        {
            this.funcSystem = funcSystem;
            this.funcShow = funcShow;
            SquareIsMonster = new List<SquareOfAgent>();

            Start = new SquareOfAgent(StartSquare.Property, StartSquare.Item);
            Start.Location = new Point(0, 0);
            Start.SetIsSafe(true);

            Curr = Start;
            this.direction = direction;

            CreateNeighber(Curr);

            Curr.IsVisited = true;

            EventCMD = direction;
            
            funcShow.Invoke("I'm READY for TRAVEL!!!", null, Curr);
        }

        //Finding and connecting all near square in the map.

        public void ConnectNeighber(SquareOfAgent square)
        {
            int X = square.Location.X;
            int Y = square.Location.Y;

            Point top = new Point(X, Y - 1);
            Point bot = new Point(X, Y + 1);
            Point left = new Point(X - 1, Y);
            Point right = new Point(X + 1, Y);

            SquareOfAgent SquareTop = FindMatch(Curr, top);
            SquareOfAgent SquareBot = FindMatch(Curr, bot);
            SquareOfAgent SquareLeft = FindMatch(Curr, left);
            SquareOfAgent SquareRight = FindMatch(Curr, right);

            if(SquareTop != null)
            {
                SquareTop.Bot = square;
                square.Top = SquareTop;
            }

            if (SquareBot != null)
            {
                SquareBot.Top = square;
                square.Bot = SquareBot;
            }

            if (SquareLeft != null)
            {
                SquareLeft.Right = square;
                square.Left = SquareLeft;
            }

            if (SquareRight != null)
            {
                SquareRight.Left = square;
                square.Right = SquareRight;
            }
        }

        //Finding the square as same as location.

        public SquareOfAgent FindMatch(SquareOfAgent squareCurr, Point location)
        {
            if (squareCurr == null)
                return null;

            SquareOfAgent result = null;

            findSquareWithLocation(new List<SquareOfAgent>(), ref result, squareCurr, location);

            return result;
        }

        // Creating the new neighber square TOP, BOT, LEFT, RIGHT of the nSquare.
        public void CreateNeighber(SquareOfAgent nSquare)
        {
            if (nSquare.IsVisited)
            {
                Console.WriteLine("Isvisited!");
                return;
            }

            List<SquareOfAgent> NullSquare = new List<SquareOfAgent>();

            if (nSquare.Top == null)
            {
                nSquare.Top = new SquareOfAgent();
                nSquare.Top.Location = new Point(nSquare.Location.X, nSquare.Location.Y - 1);
                nSquare.Top.SetIsSafe(false);
                ConnectNeighber(nSquare.Top);

                NullSquare.Add(nSquare.Top);
            }
            else
            {
                Console.WriteLine("Top Not null|" + nSquare.Top.Location.ToString());
            }
                

            if (nSquare.Bot == null)
            {
                nSquare.Bot = new SquareOfAgent();
                nSquare.Bot.Location = new Point(nSquare.Location.X, nSquare.Location.Y + 1);
                nSquare.Bot.SetIsSafe(false);
                ConnectNeighber(nSquare.Bot);

                NullSquare.Add(nSquare.Bot);
            }
            else
                Console.WriteLine("Bot Not null|" + nSquare.Bot.Location.ToString());

            if (nSquare.Left == null)
            {
                nSquare.Left = new SquareOfAgent();
                nSquare.Left.Location = new Point(nSquare.Location.X - 1, nSquare.Location.Y);
                nSquare.Left.SetIsSafe(false);
                ConnectNeighber(nSquare.Left);

                NullSquare.Add(nSquare.Left);
            }
            else
                Console.WriteLine("Left Not null|" + nSquare.Left.Location.ToString());

            if (nSquare.Right == null)
            {
                nSquare.Right = new SquareOfAgent();
                nSquare.Right.Location = new Point(nSquare.Location.X + 1, nSquare.Location.Y);
                nSquare.Right.SetIsSafe(false);
                ConnectNeighber(nSquare.Right);

                NullSquare.Add(nSquare.Right);
            }
            else
                Console.WriteLine("right Not null|" + nSquare.Right.Location.ToString());

            if (nSquare.Property.Stench)
            {
                if(!Shoted){
                    if (SquareIsMonster.Count == 0)
                        SquareIsMonster.AddRange(NullSquare);                    
                    else
                    {
                        foreach (SquareOfAgent square in SquareIsMonster)
                        {
                            if (square.Equals(nSquare.Top) || square.Equals(nSquare.Bot)
                                || square.Equals(nSquare.Left) || square.Equals(nSquare.Right))
                            {
                                SquareIsMonster.Clear();
                                SquareIsMonster.Add(square);
                                break;
                            }
                        }
                    }
                }
            }
            else if(!MonsterIsDead)
            {
                foreach (SquareOfAgent square in SquareIsMonster)
                {
                    if (square.Equals(nSquare.Top) || square.Equals(nSquare.Bot)
                        || square.Equals(nSquare.Left) || square.Equals(nSquare.Right))
                    {
                        SquareIsMonster.Remove(square);
                        break;
                    }
                }
            }
            
            if (nSquare.Property.Breeze)
            {
                SettingBreeze(nSquare);
            }
            
            if (!nSquare.Property.Breeze && (!nSquare.Property.Stench || MonsterIsDead))
            {
                Console.WriteLine("All RIght");
                nSquare.Top.SetIsSafe(true);
                nSquare.Bot.SetIsSafe(true);
                nSquare.Left.SetIsSafe(true);
                nSquare.Right.SetIsSafe(true);
            }
        }

        private void SettingBreeze(SquareOfAgent square)
        {
            List<SquareOfAgent> SquareIsHole = new List<SquareOfAgent>();

            if(square.Top != null)
            {
                if (!square.Top.IsVisited && !square.Top.GetIsSafe())
                    SquareIsHole.Add(square.Top);
            }

            if (square.Bot != null)
            {
                if (!square.Bot.IsVisited && !square.Bot.GetIsSafe())
                    SquareIsHole.Add(square.Bot);
            }

            if (square.Right != null)
            {
                if (!square.Right.IsVisited && !square.Right.GetIsSafe())
                    SquareIsHole.Add(square.Right);
            }

            if (square.Left != null)
            {
                if (!square.Left.IsVisited && !square.Left.GetIsSafe())
                    SquareIsHole.Add(square.Left);
            }

            if (SquareIsHole.Count == 1)
            {
                SquareIsHole[0].IsHoleTime = int.MaxValue;
            }
            else
            {
                foreach (SquareOfAgent nSquare in SquareIsHole)
                {
                    nSquare.TickIsHole();
                    nSquare.SetIsSafe(false);
                    Console.WriteLine(nSquare.Location + "Breezeee - "+ nSquare.IsHoleTime);
                }
            }
        }

        private SquareOfAgent SquareMatchOfDir()
        {
            if (direction.Equals(Events.TOP))
                return Curr.Top;

            if (direction.Equals(Events.BOT))
                return Curr.Bot;

            if (direction.Equals(Events.LEFT))
                return Curr.Left;

            if (direction.Equals(Events.RIGHT))
                return Curr.Right;

            return null;
        }

        private int ShortestDistanceToPoint(Point end, Point curSquare)
        {
            if (curSquare == null)
                return int.MaxValue;

            return Math.Abs(curSquare.X - end.X) + Math.Abs(curSquare.Y - end.Y); 
        }

        private SquareOfAgent GoBackToPointThroughtVisited(SquareOfAgent end)
        {
            SquareOfAgent result = null;

            List<SquareOfAgent> gone = new List<SquareOfAgent>();
            gone.Add(end);

            Queue<SquareOfAgent> MapTravel = new Queue<SquareOfAgent>();
            MapTravel.Enqueue(end);

            ShortestPathtoPoint(ref gone, ref result, MapTravel);

            if (result == null)
                Console.WriteLine("REsult: null");
            else
                Console.WriteLine("Result shortest: " + result.Location);

            return result;
        }

        private void ShortestPathtoPoint( ref List<SquareOfAgent> gone, ref SquareOfAgent result, Queue<SquareOfAgent> MapTravel)
        { 
            
            if (result != null)
                return;

            if (MapTravel.Count == 0)
                return;

            SquareOfAgent CurrSquare = MapTravel.Dequeue();

            List<SquareOfAgent> nonGone = new List<SquareOfAgent>();

            if(CurrSquare.Top != null)
            {
                if (!gone.Contains(CurrSquare.Top))
                {
                    if (CurrSquare.Top.Equals(Curr))
                    {
                        result = CurrSquare;
                        return;
                    }

                    gone.Add(CurrSquare.Top);

                    if (CurrSquare.Top.GetIsSafe())
                    {
                        nonGone.Add(CurrSquare.Top);
                    }
                }
            }

            if (CurrSquare.Bot != null)
            {
                if (!gone.Contains(CurrSquare.Bot))
                {
                    if (CurrSquare.Bot.Equals(Curr))
                    {
                        result = CurrSquare;
                        return;
                    }

                    gone.Add(CurrSquare.Bot);

                    if (CurrSquare.Bot.GetIsSafe())
                        nonGone.Add(CurrSquare.Bot);
                }
            }

            if (CurrSquare.Left != null)
            {
                if (!gone.Contains(CurrSquare.Left))
                {
                    if (CurrSquare.Left.Equals(Curr))
                    {
                        result = CurrSquare;
                        return;
                    }

                    gone.Add(CurrSquare.Left);

                    if (CurrSquare.Left.GetIsSafe())
                        nonGone.Add(CurrSquare.Left);
                }
            }

            if (CurrSquare.Right != null)
            {
                if (!gone.Contains(CurrSquare.Right))
                {
                    if (CurrSquare.Right.Equals(Curr))
                    {
                        result = CurrSquare;
                        return;
                    }

                    gone.Add(CurrSquare.Right);

                    if (CurrSquare.Right.GetIsSafe())
                        nonGone.Add(CurrSquare.Right);
                }
            }



            foreach (SquareOfAgent square in nonGone)
                MapTravel.Enqueue(square);

            ShortestPathtoPoint(ref gone, ref result, MapTravel);            
        }

        private void findSquareWithLocation(List<SquareOfAgent> gone, ref SquareOfAgent result ,SquareOfAgent SquareCurr, Point end) 
        {
            if (result != null)
                return;

            if (SquareCurr == null)
                return;

            if (gone.Contains(SquareCurr))
                return;

            gone.Add(SquareCurr);

            if (SquareCurr.Location.X == end.X && SquareCurr.Location.Y == end.Y)
            {
                result = SquareCurr;
                return;
            }

            findSquareWithLocation(gone, ref result, SquareCurr.Top, end);
            findSquareWithLocation(gone, ref result, SquareCurr.Bot, end);
            findSquareWithLocation(gone, ref result, SquareCurr.Right, end);
            findSquareWithLocation(gone, ref result, SquareCurr.Left, end);

        }

        private String DirectionToStart()
        {
            if(Curr.Top != null)
                if (Curr.Top.Equals(Start))
                    return Events.TOP;

            if (Curr.Bot != null)
                if (Curr.Bot.Equals(Start))
                    return Events.BOT;

            if (Curr.Left != null)
                if (Curr.Left.Equals(Start))
                    return Events.LEFT;

            if (Curr.Right != null)
                if (Curr.Right.Equals(Start))
                    return Events.RIGHT;

            SquareOfAgent square = GoBackToPointThroughtVisited(Start);

            return findDirection(Curr, square);
        }


        private String findDirection(SquareOfAgent start, SquareOfAgent end)
        {
            if(start.Top != null)
                if (start.Top.Equals(end))
                    return Events.TOP;
            if (start.Bot != null)
                if (start.Bot.Equals(end))
                    return Events.BOT;
            if (start.Left != null)
                if (start.Left.Equals(end))
                    return Events.LEFT;
            if (start.Right != null)
                if (start.Right.Equals(end))
                    return Events.RIGHT;

            return "";
        }

        private Boolean ExistNonVisitAndSafe(ref String cmd) //VAn de o cho~: nếu ô đó null mà checkExistNonVisitAndSafe() lấy ô đó làm chỗ đến nên tìm k ra.
        {
            SquareOfAgent result = null;

            if(SquareNonVaS == null)
            {
                if (!checkExistNonVisitAndSafe(new List<SquareOfAgent>(), ref result, Curr))
                {
                    Console.WriteLine("Don't find ExistNonVisitAndSafe");
                    return false;
                }
                SquareNonVaS = result;
            }
            else
            {
                result = SquareNonVaS;
            }
            
            if(Curr.Top != null)
                if (Curr.Top.Location.X == result.Location.X && Curr.Top.Location.Y == result.Location.Y)
                {
                    cmd = Events.TOP;
                    if(direction.Equals(Events.TOP))
                        SquareNonVaS = null;
                    return true;
                }

            if (Curr.Bot != null)
                if (Curr.Bot.Location.X == result.Location.X && Curr.Bot.Location.Y == result.Location.Y)
                {
                    cmd = Events.BOT;
                    if (direction.Equals(Events.BOT))
                        SquareNonVaS = null;
                    return true;
                }

            if (Curr.Left != null)
                if (Curr.Left.Location.X == result.Location.X && Curr.Left.Location.Y == result.Location.Y)
                {
                    cmd = Events.LEFT;
                    if (direction.Equals(Events.LEFT))
                        SquareNonVaS = null;
                    return true;
                }
            if (Curr.Right != null)
                if (Curr.Right.Location.X == result.Location.X && Curr.Right.Location.Y == result.Location.Y)
                {
                    cmd = Events.RIGHT;
                    if (direction.Equals(Events.RIGHT))
                        SquareNonVaS = null;
                    return true;
                }

            Console.WriteLine("It's far. " + result.Location.ToString() + " " + Curr.Location.ToString());
            
            
            SquareOfAgent square = GoBackToPointThroughtVisited(result);

            cmd = findDirection(Curr, square);

            return true;
        }

        private Boolean checkExistNonVisitAndSafe(List<SquareOfAgent> gone, ref SquareOfAgent result, SquareOfAgent square)
        {
            if (result != null)
                return false;

            if (square == null)
                return false;

            if (gone.Contains(square))
                return false;

            if (!square.IsVisited && square.GetIsSafe()) {
                if (result != null)
                    return false;

                result = square;
                return true;
            }

            gone.Add(square);

            return checkExistNonVisitAndSafe(gone, ref result, square.Top) || checkExistNonVisitAndSafe(gone, ref result, square.Right)
                || checkExistNonVisitAndSafe(gone, ref result, square.Bot) || checkExistNonVisitAndSafe(gone, ref result, square.Left);
        }

        private String findTheMonster()
        {
            if (SquareIsMonster.Count > 1)
                return "";

            if (SquareIsMonster.Count == 0)
                return "";

            if (Shoted)
                return "";

            SquareOfAgent isMonster = SquareIsMonster[0];

            if(Curr.Top != null)
                if (Curr.Top.Equals(isMonster))
                    return Events.TOP;

            if (Curr.Bot != null)
                if (Curr.Bot.Equals(isMonster))
                    return Events.BOT;

            if (Curr.Left != null)
                if (Curr.Left.Equals(isMonster))
                    return Events.LEFT;

            if (Curr.Right != null)
                if (Curr.Right.Equals(isMonster))
                 return Events.RIGHT;

            return "";

        }

        private String findTheBestSafety()
        {
            SquareOfAgent result = new SquareOfAgent();
            result.IsHoleTime = int.MaxValue;

            if (SquareNonVaSBetter == null)
            {
                if(SquareIsMonster.Count == 1)
                {
                    result = SquareIsMonster[0];
                }
                else
                    findBestSafe(new List<SquareOfAgent>(), ref result, Curr);

                SquareNonVaSBetter = result;
            }
            else
                result = SquareNonVaSBetter;

            if (Curr.Top != null)
                if (Curr.Top.Location.X == result.Location.X && Curr.Top.Location.Y == result.Location.Y)
                {
                    if (direction.Equals(Events.TOP))
                        SquareNonVaSBetter = null;
                    return Events.TOP;
                }

            if (Curr.Bot != null)
                if (Curr.Bot.Location.X == result.Location.X && Curr.Bot.Location.Y == result.Location.Y)
                {
                    if (direction.Equals(Events.BOT))
                        SquareNonVaSBetter = null;
                    return Events.BOT;
                }
            if (Curr.Left != null)
                if (Curr.Left.Location.X == result.Location.X && Curr.Left.Location.Y == result.Location.Y)
                {
                    if (direction.Equals(Events.LEFT))
                        SquareNonVaSBetter = null;
                    return Events.LEFT;
                }
            if (Curr.Right != null)
                if (Curr.Right.Location.X == result.Location.X && Curr.Right.Location.Y == result.Location.Y)
                {
                    if (direction.Equals(Events.RIGHT))
                        SquareNonVaSBetter = null;
                    return Events.RIGHT;
                }

            Console.WriteLine("It's far. " + result.Location.ToString() + " " + Curr.Location.ToString());

            SquareOfAgent square = GoBackToPointThroughtVisited(result);

            return findDirection(Curr, square);
        }

        private void findBestSafe(List<SquareOfAgent> gone, ref SquareOfAgent result, SquareOfAgent square)
        {
            if (square == null)
                return;

            if (gone.Contains(square))
                return;

            if (!square.IsVisited && square.IsHoleTime < result.IsHoleTime)
            {
                result = square;
            }
                
            gone.Add(square);

            findBestSafe(gone, ref result, square.Top);
            findBestSafe(gone, ref result, square.Bot);
            findBestSafe(gone, ref result, square.Left);
            findBestSafe(gone, ref result, square.Right);
        }

        private String NextStep()
        {
            if (haveGold)
            {
                String tmp = DirectionToStart();

                if(direction == tmp)
                {
                    return Events.GO;
                }

                direction = tmp;
                return direction;
            }

            String isMonterExist = findTheMonster();

            if (!isMonterExist.Equals("") && !MonsterIsDead)
            {
                if(direction.Equals(isMonterExist))
                {
                    return Events.SHOT;
                }

                direction = isMonterExist;
                return direction;
            }

            if (Curr.Item.Equals(Items.GOLD))
                return Events.CATCH;

            String cmdTmp = "";

            if (ExistNonVisitAndSafe( ref cmdTmp))
                if (!cmdTmp.Equals(""))
                {
                    Console.WriteLine("ExistNonVisitAndSafe - " + cmdTmp);

                    if (direction.Equals(cmdTmp))
                        return Events.GO;

                    direction = cmdTmp;
                    return direction;
                }
                    
                else
                    Console.WriteLine("ExistNonVisitAndSafe - Can't find direction");

            cmdTmp = findTheBestSafety();

            if (!cmdTmp.Equals(""))
            {

                Console.WriteLine("findTheBestSafety - " + cmdTmp);

                if (direction.Equals(cmdTmp))
                {
                    if (SquareIsMonster.Contains(SquareMatchOfDir()) && !Shoted)
                        return Events.SHOT;

                    return Events.GO;
                }

                direction = cmdTmp;
                return direction;
            }

            else
                Console.WriteLine("FindBetterWays - Can't find direction");

            return cmdTmp;
        }

        private void SetSquareIsNull(SquareOfAgent squareNull)
        {
            FindAndSetSquareIsNull(new List<SquareOfAgent>(), squareNull, ref Curr);
        }

        private void FindAndSetSquareIsNull(List<SquareOfAgent> gone, SquareOfAgent squareNull, ref SquareOfAgent squareCurr)
        {
            if (squareCurr == null)
                return;

            if (gone.Contains(squareCurr))
                return;

            if (squareCurr.Top != null)
                if (squareCurr.Top.Equals(squareNull))
                    squareCurr.Top = null;

            if (squareCurr.Bot != null)
                if (squareCurr.Bot.Equals(squareNull))
                    squareCurr.Bot = null;

            if (squareCurr.Left != null)
                if (squareCurr.Left.Equals(squareNull))
                    squareCurr.Left = null;

            if (squareCurr.Right != null)
                if (squareCurr.Right.Equals(squareNull))
                    squareCurr.Right = null;

            gone.Add(squareCurr);

            FindAndSetSquareIsNull(gone, squareNull, ref squareCurr.Top);
            FindAndSetSquareIsNull(gone, squareNull, ref squareCurr.Bot);
            FindAndSetSquareIsNull(gone, squareNull, ref squareCurr.Left);
            FindAndSetSquareIsNull(gone, squareNull, ref squareCurr.Right);
        }

        private void SetAllAfterMonsterDead(SquareOfAgent squareCurr)
        {
            if (squareCurr == null)
                return;

            if (!squareCurr.IsVisited)
                return;

            if (!squareCurr.Property.Breeze)
            {
                if (squareCurr.Top != null)
                    squareCurr.Top.SetIsSafe(true);

                if (squareCurr.Bot != null)
                    squareCurr.Bot.SetIsSafe(true);

                if (squareCurr.Left != null)
                    squareCurr.Left.SetIsSafe(true);

                if (squareCurr.Right != null)
                    squareCurr.Right.SetIsSafe(true);
            }
        }

        private void UpdateBreeze(SquareOfAgent squareCurr)
        {
            List<SquareOfAgent> list = new List<SquareOfAgent>();

            if (squareCurr.Top !=  null)
            {
                if(!squareCurr.Top.IsVisited && !squareCurr.GetIsSafe())
                    list.Add(squareCurr.Top);
            }

            if (squareCurr.Bot != null)
            {
                if (!squareCurr.Bot.IsVisited && !squareCurr.GetIsSafe())
                    list.Add(squareCurr.Bot);
            }

            if (squareCurr.Left !=  null)
            {
                if(!squareCurr.Left.IsVisited && !squareCurr.GetIsSafe())
                    list.Add(squareCurr.Left);
            }

            if (squareCurr.Right != null)
            {
                if (!squareCurr.Right.IsVisited && !squareCurr.GetIsSafe())
                    list.Add(squareCurr.Right);
            }

            if (list.Count > 1 || list.Count == 0)
                return;

            list[0].IsHoleTime = int.MaxValue;
            list[0].SetIsSafe(false);
        }

        //The method receive data from SystemMaster. Object can be new squares, null squares or monster is died (boolean). 
        public void ExcuteEvent(Object Result, Boolean TheEnd)
        {
            _manualResetEvent.WaitOne(Timeout.Infinite);

            if (!_running)
            {
                return;
            }

            if (EventCMD.Equals(Events.CATCH))
            {
                haveGold = (Boolean)Result;
                if (haveGold)
                {
                    Curr.Item = "";

                    funcShow.Invoke("I carried GOLD!", Start, Curr);
                }
                else
                {
                    funcShow.Invoke("I missed GOLD!", Start, Curr);
                }

                if (MainScreen.speed > 0)
                    Thread.Sleep(1000);
            }

            if (EventCMD.Equals(Events.SHOT))
            {
                MonsterIsDead = (Boolean)Result;
                Console.WriteLine("Shot: " + MonsterIsDead);

                if (MonsterIsDead)
                {
                    SquareOfAgent nSquare = SquareIsMonster[0];
                    nSquare.SetIsSafe(true);

                    SetAllAfterMonsterDead(nSquare.Top);
                    SetAllAfterMonsterDead(nSquare.Bot);
                    SetAllAfterMonsterDead(nSquare.Left);
                    SetAllAfterMonsterDead(nSquare.Right);

                    SquareIsMonster.Clear();

                    funcShow.Invoke("The Monster is DEAD!", SquareMatchOfDir(), Curr);
                }
                else
                {
                    SquareOfAgent nSquare = SquareIsMonster[0];
                    nSquare.IsHoleTime = 3;

                    funcShow.Invoke("The Monster is LIVE!", SquareMatchOfDir(), Curr);
                }

                if (MainScreen.speed > 0)
                    Thread.Sleep(1000);

                Shoted = true;
            }

            if (EventCMD.Equals(Events.GO))
            {
                if(Result == null)
                {
                    SquareOfAgent squareNull = SquareMatchOfDir();

                    //Set all square have as same as position is null 
                    SetSquareIsNull(squareNull);

                    if (Curr.Property.Breeze)
                        UpdateBreeze(Curr);

                    foreach (SquareOfAgent square in SquareIsMonster)
                    {
                        if (squareNull.Equals(square))
                        {
                            SquareIsMonster.Remove(square);
                            break;
                        }
                    }

                    if (SquareNonVaS != null)
                        funcShow.Invoke("Bump!", SquareNonVaS, Curr);
                    else
                        funcShow.Invoke("Bump!", SquareNonVaSBetter, Curr);

                    if(MainScreen.speed > 0)
                        Thread.Sleep(1000);
                }
                else
                {

                    Squares nSquare = (Squares)Result;

                    SquareOfAgent newSquare = SquareMatchOfDir();

                    if (newSquare.IsVisited)
                    {
                        Curr = newSquare;
                        EventCMD = NextStep();

                        if (TheEnd)
                        {
                            if (Curr.Equals(Start))
                            {
                                funcShow.Invoke("I'm Winner!", Curr, Curr);
                            }

                            return;
                        }
                        else if (haveGold)
                            funcShow.Invoke(ConvertEvents(EventCMD), Start, Curr);
                        else if (SquareNonVaS != null)
                            funcShow.Invoke(ConvertEvents(EventCMD), SquareNonVaS, Curr);
                        else
                            funcShow.Invoke(ConvertEvents(EventCMD), SquareNonVaSBetter, Curr);

                        Thread.Sleep(MainScreen.speed);

                        funcSystem.Invoke(EventCMD);
                        return;
                    }

                    newSquare.Item = nSquare.Item;
                    newSquare.Property = nSquare.Property;
                    
                    CreateNeighber(newSquare);

                    newSquare.IsVisited = true;
                    newSquare.SetIsSafe(true);
                    Curr = newSquare;
                }
            }

            EventCMD = NextStep();

            if (TheEnd)
            {
                if (Curr.Item == Items.HOLE || Curr.Item == Items.MONSTER)
                {
                    funcShow.Invoke("I'm Closer!", Curr, Curr);
                }
                return;
            }
            else if (haveGold)
                funcShow.Invoke(ConvertEvents(EventCMD), Start, Curr);
            else if (SquareNonVaS != null)
                funcShow.Invoke(ConvertEvents(EventCMD), SquareNonVaS, Curr);
            else
                funcShow.Invoke(ConvertEvents(EventCMD), SquareNonVaSBetter, Curr);

            Thread.Sleep(MainScreen.speed);
            funcSystem.Invoke(EventCMD);
        }
        
        public void PausePlay()
        {
           
            _manualResetEvent.Reset();
        }

        public void ResumePlay()
        {
            _manualResetEvent.Set();            
        }

        public void StopPlay()
        {
            _running = false;
            ResumePlay();
        }

        public void StartGame()
        {
            ExcuteEvent(null, false);
        }

        private String ConvertEvents(String cmd)
        {
            if (cmd.Equals(Events.CATCH))
            {
                return "Yeah! I found the GOLD! I will CATCH it.";
            }

            if (cmd.Equals(Events.BOT))
            {
                return "I'm turn BOT";
            }

            if (cmd.Equals(Events.TOP))
            {
                return "I'm turn TOP";
            }

            if (cmd.Equals(Events.LEFT))
            {
                return "I'm turn LEFT";
            }

            if (cmd.Equals(Events.RIGHT))
            {
                return "I'm turn RIGHT";
            }

            if (cmd.Equals(Events.SHOT))
            {
                return "I know MONSTER on " + direction + ". I will SHOT it.";
            }

            if (cmd.Equals(Events.GO))
            {
                return "I'm GO " + direction + ".";
            }

            return "I'm Surrender.";
        }
        
    }
}
