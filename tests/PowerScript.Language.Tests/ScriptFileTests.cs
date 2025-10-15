using NUnit.Framework;

namespace PowerScript.Language.Tests;

/// <summary>
/// Tests that execute actual PowerScript files from the test-scripts folder.
/// These tests validate that real-world script examples work correctly.
/// </summary>
[TestFixture]
public class ScriptFileTests : LanguageTestBase
{
    private string GetScriptPath(string category, string filename)
    {
        // test-scripts folder is at repository root
        // TestDirectory is: tests/PowerScript.Language.Tests/bin/Debug/net8.0
        // We need to go up 5 levels to get to repository root
        var baseDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "test-scripts");
        var fullPath = Path.GetFullPath(baseDir);
        return Path.Combine(fullPath, category, filename);
    }

    private void ExecuteScriptFile(string category, string filename)
    {
        var path = GetScriptPath(category, filename);
        Assert.That(File.Exists(path), Is.True, $"Script file not found: {path}");

        var script = File.ReadAllText(path);
        Interpreter.ExecuteCode(script);
    }

    #region Language Feature Tests (language/*.ps)

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_1_FlexType()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "1_flex_type.ps"));
        var output = GetOutput();
        Assert.That(output, Does.Contain("10"));
        Assert.That(output, Does.Contain("hello"));
        Assert.That(output, Does.Contain("3.14"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_2_VarType()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "2_var_type.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_3_IntType()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "3_int_type.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_4_StringType()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "4_string_type.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_5_NumberType()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "5_number_type.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_6_PrecType()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "6_prec_type.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_7_BitWidth_Int8()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "7_bitwidth_int8.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_8_BitWidth_Int16()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "8_bitwidth_int16.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_9_BitWidth_Number()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "9_bitwidth_number.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_10_BitWidth_Arithmetic()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "10_bitwidth_arithmetic.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_11_CycleLoop()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "11_cycle_loop.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_12_IfElse()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "12_if_else.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_13_ComparisonOperators()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "13_comparison_operators.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_14_LogicalAnd()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "14_logical_and.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_15_LogicalOr()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "15_logical_or.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_16_ArithmeticOperators()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "16_arithmetic_operators.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_17_NestedLoops()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "17_nested_loops.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_18_NestedConditionals()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "18_nested_conditionals.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_19_OperatorPrecedence()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "19_operator_precedence.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_20_PrintStatement()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "20_print_statement.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_21_StringLiterals()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "21_string_literals.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_22_VariableReassignment()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "22_variable_reassignment.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_23_MixedTypes()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "23_mixed_types.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_24_BitWidthMixed()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "24_bitwidth_mixed.ps"));
    }

    [Test]
    [Category("Language")]
    [Category("ScriptFile")]
    public void Language_25_ComplexExpression()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("language", "25_complex_expression.ps"));
    }

    #endregion

    #region Simple Tests (1_*.ps)

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_1_Variables()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_1_variables.ps"));
        var output = GetOutput();
        Assert.That(output, Does.Contain("5"));
        Assert.That(output, Does.Contain("10"));
        Assert.That(output, Does.Contain("15"));
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_2_Arithmetic()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_2_arithmetic.ps"));
        var output = GetOutput();
        Assert.That(output, Does.Contain("13")); // 10 + 3
        Assert.That(output, Does.Contain("7"));  // 10 - 3
        Assert.That(output, Does.Contain("30")); // 10 * 3
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_3_Conditional_Simple()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_3_conditional_simple.ps"));
        var output = GetOutput();
        Assert.That(output, Does.Contain("GREATER THAN 5").Or.Contains("NOT GREATER THAN 5"));
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_4_Loop_Simple()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_4_loop_simple.ps"));
        var output = GetOutput();
        Assert.That(output, Does.Contain("5"));
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_5_Loop_Counter()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_5_loop_counter.ps"));
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_6_Factorial()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_6_factorial.ps"));
        var output = GetOutput();
        // 5! = 120
        Assert.That(output, Does.Contain("120"));
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_7_Parentheses()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_7_parentheses.ps"));
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_8_Boolean_And()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_8_boolean_and.ps"));
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_9_Boolean_Or()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_9_boolean_or.ps"));
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_10_Nested_Conditionals()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_10_nested_conditionals.ps"));
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_11_Nested_Loops()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_11_nested_loops.ps"));
    }

    [Test]
    [Category("Simple")]
    [Category("ScriptFile")]
    public void Simple_1_12_Loop_With_Conditional()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("simple", "1_12_loop_with_conditional.ps"));
    }

    #endregion

    #region Moderate Tests (2_*.ps)

    [Test]
    [Category("Moderate")]
    [Category("ScriptFile")]
    public void Moderate_2_1_Sum_Of_Numbers()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("moderate", "2_1_sum_of_numbers.ps"));
        var output = GetOutput();
        // Sum of 1 to 10 = 55
        Assert.That(output, Does.Contain("55"));
    }

    [Test]
    [Category("Moderate")]
    [Category("ScriptFile")]
    public void Moderate_2_2_Conditional_Counting()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("moderate", "2_2_conditional_counting.ps"));
    }

    [Test]
    [Category("Moderate")]
    [Category("ScriptFile")]
    public void Moderate_2_3_Even_Odd_Sum()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("moderate", "2_3_even_odd_sum.ps"));
    }

    [Test]
    [Category("Moderate")]
    [Category("ScriptFile")]
    public void Moderate_2_4_Multiplication_Table_Sum()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("moderate", "2_4_multiplication_table_sum.ps"));
    }

    [Test]
    [Category("Moderate")]
    [Category("ScriptFile")]
    public void Moderate_2_5_Fibonacci()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("moderate", "2_5_fibonacci.ps"));
    }

    [Test]
    [Category("Moderate")]
    [Category("ScriptFile")]
    public void Moderate_2_6_Power_Calculation()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("moderate", "2_6_power_calculation.ps"));
    }

    [Test]
    [Category("Moderate")]
    [Category("ScriptFile")]
    public void Moderate_2_7_Find_Maximum()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("moderate", "2_7_find_maximum.ps"));
    }

    [Test]
    [Category("Moderate")]
    [Category("ScriptFile")]
    public void Moderate_2_8_Count_Primes()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("moderate", "2_8_count_primes.ps"));
    }

    [Test]
    [Category("Moderate")]
    [Category("ScriptFile")]
    public void Moderate_2_9_Complex_Expressions()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("moderate", "2_9_complex_expressions.ps"));
    }

    [Test]
    [Category("Moderate")]
    [Category("ScriptFile")]
    public void Moderate_2_10_Sum_And_Product()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("moderate", "2_10_sum_and_product.ps"));
    }

    #endregion

    #region Complex Tests (3_*.ps)

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_1_Bubble_Sort()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_1_bubble_sort.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_1_Bubble_Sort_Stats()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_1_bubble_sort_stats.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_2_Matrix_Operations()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_2_matrix_operations.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_2_State_Machine()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_2_state_machine.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_3_Maze_Solver()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_3_maze_solver.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_4_Auto_Generated_Variables()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_4_auto_generated_variables.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_5_Triple_Nested_Loops()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_5_triple_nested_loops.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_6_Mixed_Loop_Variables()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_6_mixed_loop_variables.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_7_Factorial()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_7_factorial.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_8_Fibonacci()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_8_fibonacci.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_9_Prime_Detection()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_9_prime_detection.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_10_GCD()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_10_gcd.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_11_Nested_Conditions()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_11_nested_conditions.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_12_Loop_Breaking()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_12_loop_breaking.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_13_Stack_Simulation()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_13_stack_simulation.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_14_Queue_Simulation()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_14_queue_simulation.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_15_Power_Function()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_15_power_function.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_16_Square_Root()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_16_square_root.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_17_Array_Reversal()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_17_array_reversal.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_18_Linear_Search()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_18_linear_search.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_19_Deep_Nesting()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_19_deep_nesting.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_20_Large_Array()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_20_large_array.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_21_Zero_Iterations()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_21_zero_iterations.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_22_Division_Modulo()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_22_division_modulo.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_23_Operator_Precedence()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_23_operator_precedence.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_24_Integration_Test()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_24_integration_test.ps"));
    }

    [Test]
    [Category("Complex")]
    [Category("ScriptFile")]
    public void Complex_3_25_Collatz_Sequence()
    {
        Assert.DoesNotThrow(() => ExecuteScriptFile("complex", "3_25_Collatz_sequence.ps"));
    }

    #endregion
}
