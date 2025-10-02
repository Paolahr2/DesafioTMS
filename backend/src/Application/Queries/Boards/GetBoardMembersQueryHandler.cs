using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.Boards;

/// <summary>
/// Handler para GetBoardMembersQuery
/// </summary>
public class GetBoardMembersQueryHandler : IRequestHandler<GetBoardMembersQuery, List<BoardMemberDto>>
{
    private readonly BoardRepository _boardRepository;
    private readonly UserRepository _userRepository;

    public GetBoardMembersQueryHandler(
        BoardRepository boardRepository,
        UserRepository userRepository)
    {
        _boardRepository = boardRepository;
        _userRepository = userRepository;
    }

    public async Task<List<BoardMemberDto>> Handle(GetBoardMembersQuery request, CancellationToken cancellationToken)
    {
        // Obtener el tablero
        var board = await _boardRepository.GetByIdAsync(request.BoardId);
        if (board == null)
        {
            return new List<BoardMemberDto>();
        }

        var members = new List<BoardMemberDto>();

        // Agregar el owner como miembro con rol Owner
        var owner = await _userRepository.GetByIdAsync(board.OwnerId);
        if (owner != null)
        {
            members.Add(new BoardMemberDto
            {
                UserId = board.OwnerId,
                BoardId = board.Id,
                Role = "Owner",
                JoinedAt = board.CreatedAt,
                UserName = owner.Username,
                UserEmail = owner.Email,
                UserAvatar = null,
                FullName = owner.FullName ?? owner.Username
            });
        }

        // Agregar los miembros adicionales con rol Member
        foreach (var memberId in board.MemberIds)
        {
            if (memberId != board.OwnerId) // Evitar duplicar el owner
            {
                var member = await _userRepository.GetByIdAsync(memberId);
                if (member != null)
                {
                    members.Add(new BoardMemberDto
                    {
                        UserId = memberId,
                        BoardId = board.Id,
                        Role = "Member",
                        JoinedAt = board.CreatedAt,
                        UserName = member.Username,
                        UserEmail = member.Email,
                        UserAvatar = null,
                        FullName = member.FullName ?? member.Username
                    });
                }
            }
        }

        return members;
    }
}
