namespace Api.Swazy.Models.Results;

public class CommonResponse<T>
{
    public CommonResult Result { get; set; }

    public T? Value { get; set; }
}