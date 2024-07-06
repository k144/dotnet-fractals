using System.Text;
using SkiaSharp;

class Program
{
    
    public class Fractal(string Name, string Axiom, int InitialAngle,  Dictionary<char, string> Rules)
    {
        public string Name { get; set; } = Name;
        public string Axiom { get; set; } = Axiom;
        public int InitialAngle { get; set; } = InitialAngle;
        public Dictionary<char, string> Rules { get; set; } = Rules;

    }

    public class GameState(List<Fractal> Fractals, int SelectedIndex, Fractal Selected, string generated, int Angle, int Iterations, int Length)
    {
        public List<Fractal> Fractals { get; set; } = Fractals;
        public int SelectedIndex { get; set; } = SelectedIndex;
        public Fractal Selected { get; set; } = Selected;
        public string Generated { get; set; } = generated;
        public int Angle { get; set; } = Angle;
        public int Iterations { get; set; } = Iterations;
        public double Length { get; set; } = Length;
        public int StartAngle { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int EditIndex { get; set; }
        public bool EditRuleChar { get; set; }

    }


    static void Main(string[] args)
    {
        var fractals  = new List<Fractal>()
        {
            new("Roślinka", "X", 25, new()
            { 
                { 'X', "F+[[X]-X]-F[-FX]+X)" },
                { 'F', "FF" }
            }),
            new("Smok", "F", 90, new()
            {
                {'F', "F+G"},
                {'G', "F-G"}
            }),
            new("Sierpiński", "A", 60, new()
            {
                {'A', "B-A-B"},
                {'B', "A+B+A"}
            }),
        };


        var mode = Mode.View;
        var currentFractal = fractals[0];
        var state = new GameState(fractals, 0, currentFractal, null, currentFractal.InitialAngle, 5, 1);
        state.Generated = GenerateLSystem(state);
        DrawLSystem(state, mode);
        
        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.X && mode == Mode.View)
            {
                break;
            }

            if (mode == Mode.View && key.Key == ConsoleKey.R)
            {
                mode = Mode.Edit;
                var sel = state.Selected;
                var newFractal = new Fractal("New Fractal", sel.Axiom, sel.InitialAngle,
                    sel.Rules.ToDictionary(e => e.Key, e => e.Value));

                state.SelectedIndex = fractals.Count;
                state.Fractals.Add(newFractal);
                state.Selected = newFractal;
                
                DrawLSystem(state, mode);
                continue;
            }
            
            if (mode == Mode.Edit && key.Key == ConsoleKey.Enter)
            {
                mode = Mode.View;
                DrawLSystem(state, mode);
                continue;
            }

            switch (mode)
            {
                case Mode.View:
                    HandleViewKey(key.Key, state);
                    break;
                case Mode.Edit:
                    HandleEditKey(key, state);
                    break;
            }
            
            DrawLSystem(state, mode);
        }
        
    }

    private static void HandleEditKey(ConsoleKeyInfo keyinfo, GameState state)
    {

        var editLen = 2 + state.Selected.Rules.Count;
        
        switch (keyinfo.Key)
        {

            case ConsoleKey.DownArrow:
                state.EditIndex++;

                if (state.EditIndex == editLen)
                {
                    if (state.Selected.Rules.TryAdd(' ', string.Empty))
                    {
                        state.EditIndex -= 1;
                        state.EditRuleChar = true;
                    }
                }
                else if (state.EditIndex > editLen)
                {
                    state.EditIndex = editLen - 1;
                }
                
                break;
            
            case ConsoleKey.UpArrow:
                state.EditIndex--;
                state.Selected.Rules.Remove(' ');

                if (state.EditIndex < 0)
                {
                    state.EditIndex = 0;
                }
                break;

            case ConsoleKey.LeftArrow:
                state.EditRuleChar = true;
                break;
            
            case ConsoleKey.RightArrow:
                state.EditRuleChar = false;
                break;
            
            case ConsoleKey.Backspace:
                var frac = state.Selected;
                if (state.EditIndex == 0 && frac.Name.Length > 0)
                {
                    frac.Name = frac.Name[..^1];
                }
                else if (state.EditIndex == 1 && frac.Axiom.Length > 0)
                {
                    frac.Axiom = frac.Axiom[..^1];
                }
                else if (state.EditIndex >= 2)
                {
                    var ruleCount = frac.Rules.Count;
                    var ruleIndex = state.EditIndex - 2;
                    if (ruleIndex >= ruleCount) break;
                    var rule = frac.Rules.ElementAt(ruleIndex);

                    if (state.EditRuleChar)
                    {
                        frac.Rules.TryAdd(' ', rule.Value);
                        frac.Rules.Remove(rule.Key);
                        break;
                    }
                    if (rule.Value.Length > 0)
                    {
                        frac.Rules[rule.Key] = rule.Value[..^1];
                    }
                    
                }
                
                break;
            
            default:

                frac = state.Selected;
                var c = keyinfo.KeyChar;

                if (!char.IsAscii(c)) break;

                c = char.ToUpper(c);
                
                if (state.EditIndex == 0)
                {
                    frac.Name += c;
                }
                else if (state.EditIndex == 1)
                {
                    frac.Axiom += c;
                }
                else if (state.EditIndex >= 2)
                {
                    var ruleCount = frac.Rules.Count;
                    var ruleIndex = state.EditIndex - 2;
                    if (ruleIndex >= ruleCount) break;
                    var rule = frac.Rules.ElementAt(ruleIndex);

                    if (state.EditRuleChar)
                    {
                        if (frac.Rules.TryAdd(c, rule.Value))
                        {
                            state.EditRuleChar = false;
                            frac.Rules.Remove(rule.Key);
                        }
                        break;
                    }
                    frac.Rules[rule.Key] = rule.Value + c;
                }
                
                break;
        }
        state.Generated =  GenerateLSystem(state);
    }

    private static void HandleViewKey(ConsoleKey key, GameState state)
    {
        switch (key)
        {
            case ConsoleKey.M:
                state.SelectedIndex++;
                if (state.SelectedIndex >= state.Fractals.Count)
                    state.SelectedIndex = 0;
                state.Selected = state.Fractals[state.SelectedIndex];
                state.Angle = state.Selected.InitialAngle;
                state.Generated =  GenerateLSystem(state);
                break;
            
            case ConsoleKey.N:
                state.SelectedIndex--;
                if (state.SelectedIndex < 0)
                    state.SelectedIndex = state.Fractals.Count - 1;
                state.Selected = state.Fractals[state.SelectedIndex];
                state.Angle = state.Selected.InitialAngle;
                state.Generated =  GenerateLSystem(state);
                break;
            
            case ConsoleKey.LeftArrow:
                state.Angle -= 1;
                break;
            
            case ConsoleKey.RightArrow:
                state.Angle += 1;
                break;
            
            case ConsoleKey.UpArrow:
                state.Length *= 1.5;
                break;
            
            case ConsoleKey.DownArrow:
                state.Length /= 1.5;
                break;
            
            case ConsoleKey.Q:
                state.StartAngle -= 1;
                break;
            
            case ConsoleKey.E:
                state.StartAngle += 1;
                break;
            
            case ConsoleKey.W:
                state.OffsetY -= 1;
                break;
            
            case ConsoleKey.A:
                state.OffsetX -= 1;
                break;
            
            case ConsoleKey.S:
                state.OffsetY += 1;
                break;
            
            case ConsoleKey.D:
                state.OffsetX += 1;
                break;
            
            case ConsoleKey.I:
                state.Iterations += 1;
                state.Length /= 2;
                state.Generated =  GenerateLSystem(state);
                break;
            
            case ConsoleKey.O:
                state.Iterations -= 1;
                state.Length *= 2;
                state.Generated =  GenerateLSystem(state);
                break;
            
        }
    }

    public enum Mode
    {
        Edit,
        View,
    }

    static string GenerateLSystem(GameState state) => GenerateLSystem(state.Selected.Axiom, state.Selected.Rules, state.Iterations);
    static string GenerateLSystem(string axiom, Dictionary<char, string> rules, int iterations)
    {
        var current = axiom;

        for (var i = 0; i < iterations; i++)
        {
            var result = new StringBuilder();
            foreach (var c in current)
            {
                if (rules.TryGetValue(c, out var value))
                {
                    result.Append(value);
                }
                else
                {
                    result.Append(c);
                }
            }
            current = result.ToString();
        }

        return current;
    }


    static string GenerateEditView(GameState state)
    {
        var sb = new StringBuilder();

        var frac = state.Selected;
        var current = frac.Axiom;
        sb.AppendLine(current);
        for (var i = 0; i < 2; i++)
        {
            current = GenerateLSystem(current, frac.Rules, 1);
            sb.AppendLine(current);
        }

        sb.Append(state.EditIndex == 0 ? '>' : ' ');
        sb.AppendLine($"Name: {frac.Name}");
        sb.Append(state.EditIndex == 1 ? '>' : ' ');
        sb.AppendLine($"Axiom: {frac.Axiom}");

        var index = 2;
        foreach (var rule in frac.Rules)
        {
            sb.Append(state.EditIndex == index ? '>' : ' ');
            sb.Append(rule.Key);
            sb.Append(" -> ");
            sb.Append(rule.Value);
            sb.Append('\n');
            index++;
        }

        return sb.ToString();
    }
    
    static void DrawLSystem(GameState state, Mode? mode)
    {
        var width = Console.LargestWindowWidth;
        var height = Console.LargestWindowHeight;
        
        var editView = string.Empty;
        if (mode == Mode.Edit)
        {
            editView = GenerateEditView(state);
            height -= editView.Count(c => c == '\n');
        }
        
        
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White);

        var paint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 1,
            IsAntialias = true,
        };

        var stateStack = new Stack<(SKPoint, float)>();

        SKPoint position = new SKPoint(width / 4 + state.OffsetX, height / 2 + state.OffsetY);
        float currentAngle = state.StartAngle;

        SKPoint newPosition;
        
        foreach (var command in state.Generated)
        {
            switch (command)
            {
                case '+':
                    currentAngle += state.Angle;
                    break;

                case '-':
                    currentAngle -= state.Angle;
                    break;

                case '[':
                    stateStack.Push((position, currentAngle));
                    break;

                case ']':
                    (position, currentAngle) = stateStack.Pop();
                    break;
                
                default:
                    newPosition = new SKPoint(
                        position.X + (float)(state.Length * Math.Cos(currentAngle * Math.PI / 180)),
                        position.Y + (float)(state.Length * Math.Sin(currentAngle * Math.PI / 180))
                    );

                    var ps = new SKPoint(
                        position.X*2,
                        position.Y);
                    
                    var nps = new SKPoint(
                        newPosition.X*2,
                        newPosition.Y);
                    
                    canvas.DrawLine(ps, nps, paint);
                    position = newPosition;
                    break;
                    
            }
        }

        using var image = surface.Snapshot();
        DrawBitmapInTerminal(image);

        if (mode == Mode.Edit)
        {
            Console.Write(editView);
        }
        
        DrawMenu(state);
    }

    private static void DrawMenu(GameState state)
    {
        var sb = new StringBuilder();
        foreach (var fractal in state.Fractals)
        {
            sb.Append(fractal == state.Selected ? '<' : ' ');
            sb.Append(fractal.Name);
            sb.Append(fractal == state.Selected ? '>' : ' ');
            sb.Append(' ');
        }
        
        sb.Append($"- Kąt: {state.Angle} (Lewo Pawo) | Iteracje: {state.Iterations} (I O) | Długość: {state.Length} (Góra Dół)");
        sb.Append(" | Wyjdź: X | Obrót: Q E | Przesunięcie: W A S D ");
        
        Console.WriteLine(sb.ToString());
    }

    private static readonly string
        AsciiChars = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\\|()1{}[]?-_+~<>i!lI;:,\"^` ";
    
    public static void DrawBitmapInTerminal(SKImage image)
    {
        var bitmap = SKBitmap.FromImage(image);
        var asciiArt = new StringBuilder();
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                var gray = (int)(color.Red * 0.3 + color.Green * 0.59 + color.Blue * 0.11);
                var scaled = gray * (AsciiChars.Length - 1) / 255;
                var asciiChar = AsciiChars[scaled];
                asciiArt.Append(asciiChar);
            }
            asciiArt.AppendLine();
        }

        var buf = asciiArt.ToString().Select(e => (byte)e).ToArray();
        Console.Clear();
        Console.SetCursorPosition(0, 0);
        using var stdout = Console.OpenStandardOutput(bitmap.Height * bitmap.Width);
        stdout.Write(buf, 0, buf.Length);
    }
}
