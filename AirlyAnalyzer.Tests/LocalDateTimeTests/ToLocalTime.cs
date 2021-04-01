namespace AirlyAnalyzer.UnitTests.DateTimeTests
{
  using System;
  using Xunit;

  public class ToLocalTime
  {
    [Fact]
    public void add_one_hour_more_in_to_local_time_conversion_when_begin_of_daylight_time()
    {
      // Arrange
      var beforeTransitionTime
          = new DateTime(2001, 3, 25, 0, 0, 0, DateTimeKind.Utc);
      var transitionTime = beforeTransitionTime.AddHours(1);

      // Act
      var beforeTransitionTimeLocal = beforeTransitionTime.ToLocalTime();
      var transitionTimeLocal = transitionTime.ToLocalTime();

      int hoursDifference
          = (transitionTimeLocal - beforeTransitionTimeLocal).Hours;

      // Assert
      Assert.Equal(2, hoursDifference);
    }

    [Fact]
    public void add_one_hour_less_in_to_local_time_conversion_when_end_of_daylight_time()
    {
      // Arrange
      var beforeTransitionTime
          = new DateTime(2001, 10, 28, 0, 0, 0, DateTimeKind.Utc);
      var transitionTime = beforeTransitionTime.AddHours(1);

      // Act
      var beforeTransitionTimeLocal = beforeTransitionTime.ToLocalTime();
      var transitionTimeLocal = transitionTime.ToLocalTime();

      int hoursDifference
          = (transitionTimeLocal - beforeTransitionTimeLocal).Hours;

      // Assert
      Assert.Equal(0, hoursDifference);
    }

    [Fact]
    public void do_not_add_extra_hour_to_local_time_when_begin_of_daylight_time()
    {
      // Arrange
      var beforeTransitionTimeLocal
          = new DateTime(2001, 3, 25, 1, 0, 0, DateTimeKind.Local);
      var transitionTimeLocal = beforeTransitionTimeLocal.AddHours(1);

      // Act
      int hoursDifference
          = (transitionTimeLocal - beforeTransitionTimeLocal).Hours;

      // Assert
      Assert.Equal(1, hoursDifference);
    }

    [Fact]
    public void do_not_subtract_extra_hour_to_local_time_when_end_of_daylight_time()
    {
      // Arrange
      var beforeTransitionTimeLocal
          = new DateTime(2001, 10, 28, 2, 0, 0, DateTimeKind.Local);
      var transitionTimeLocal = beforeTransitionTimeLocal.AddHours(1);

      // Act
      int hoursDifference
          = (transitionTimeLocal - beforeTransitionTimeLocal).Hours;

      // Assert
      Assert.Equal(1, hoursDifference);
    }
  }
}
